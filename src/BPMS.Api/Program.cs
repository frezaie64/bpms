using BPMS.Modules.FileManagement;
using BPMS.Modules.FileManagement.Endpoints;
using BPMS.Modules.FileManagement.Models;
using BPMS.Modules.FormGenerator;
using BPMS.Modules.FormGenerator.Endpoints;
using BPMS.Modules.FormGenerator.Models;
using BPMS.Modules.Forms;
using BPMS.Modules.Forms.Endpoints;
using BPMS.Modules.Identity;
using BPMS.Modules.Identity.Endpoints;
using BPMS.Modules.Identity.Entities;
using BPMS.Modules.Notifications;
using BPMS.Modules.Notifications.Endpoints;
using BPMS.Modules.Reports;
using BPMS.Modules.Reports.Endpoints;
using BPMS.Modules.Tenancy;
using BPMS.Modules.Tenancy.Endpoints;
using BPMS.Modules.WorkflowDefinitions;
using BPMS.Modules.WorkflowDefinitions.Endpoints;
using BPMS.Modules.WorkflowRuntime;
using BPMS.Modules.WorkflowRuntime.Endpoints;
using BPMS.Shared.Authorization;
using BPMS.Shared.Extensions;
using BPMS.Shared.Persistence;
using BPMS.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
    config.ReadFrom.Configuration(context.Configuration));

builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddPermissionAuthorization();
builder.Services.AddCors();

builder.Services.AddDbContext<BpmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)));

builder.Services.AddIdentityModule();
builder.Services.AddTenancyModule();
builder.Services.AddFormsModule();
builder.Services.AddWorkflowDefinitionsModule();
builder.Services.AddWorkflowRuntimeModule();
builder.Services.AddNotificationsModule();
builder.Services.AddFileManagementModule();
builder.Services.AddReportsModule();
builder.Services.AddFormGeneratorModule();

builder.Services.Configure<OpenRouterOptions>(builder.Configuration.GetSection(OpenRouterOptions.SectionName));
builder.Services.Configure<MinioOptions>(builder.Configuration.GetSection(MinioOptions.SectionName));
builder.Services.Configure<FileValidationOptions>(builder.Configuration.GetSection(FileValidationOptions.SectionName));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<BpmsDbContext>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token"
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(policy =>
    policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseTenantMiddleware();
app.UseAuthorization();

app.MapAuthEndpoints();
app.MapUserEndpoints();
app.MapRoleEndpoints();
app.MapTenantEndpoints();
app.MapPlanEndpoints();
app.MapSubscriptionEndpoints();
app.MapInvitationEndpoints();
app.MapFormEndpoints();
app.MapFormCategoryEndpoints();
app.MapWorkflowEndpoints();
app.MapWorkflowInstanceEndpoints();
app.MapTaskEndpoints();
app.MapNotificationEndpoints();
app.MapAuditLogEndpoints();
app.MapFileEndpoints();
app.MapAdminEndpoints();
app.MapWorkflowCategoryEndpoints();
app.MapAdminDashboardEndpoints();
app.MapReportsEndpoints();
app.MapSearchEndpoints();
app.MapFormGenerationEndpoints();
app.MapFormSubmissionEndpoints();

app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BpmsDbContext>();
    await db.Database.MigrateAsync();

    if (!await db.Set<global::BPMS.Modules.Tenancy.Entities.Plan>().AnyAsync())
    {
        db.Set<global::BPMS.Modules.Tenancy.Entities.Plan>().AddRange(
            new global::BPMS.Modules.Tenancy.Entities.Plan
            {
                Name = "Basic",
                Slug = "basic",
                Description = "For small teams getting started",
                Price = 0,
                MaxUsers = 5,
                MaxWorkflows = 5,
                MaxAiFormGenerations = 10,
                CreatedBy = "system"
            },
            new global::BPMS.Modules.Tenancy.Entities.Plan
            {
                Name = "Pro",
                Slug = "pro",
                Description = "For growing businesses",
                Price = 29,
                MaxUsers = 20,
                MaxWorkflows = 50,
                MaxAiFormGenerations = 100,
                CreatedBy = "system"
            },
            new global::BPMS.Modules.Tenancy.Entities.Plan
            {
                Name = "Enterprise",
                Slug = "enterprise",
                Description = "For large organizations with advanced needs",
                Price = 99,
                MaxUsers = 100,
                MaxWorkflows = 500,
                MaxAiFormGenerations = 1000,
                CreatedBy = "system"
            }
        );
        await db.SaveChangesAsync();
    }

    // Seed admin user with full permissions
    // Note: TenantId defaults to "" (empty string) via TenantService,
    // which matches the tenant context during login (no X-Tenant-Id header needed).

    // Seed all permissions
    if (!await db.Set<Permission>().AnyAsync())
    {
        db.Set<Permission>().AddRange(
            Permissions.All.Select(name => new Permission { Name = name })
        );
        await db.SaveChangesAsync();
    }

    // Seed admin role (only the global role with TenantId == "" — a tenant may
    // legitimately create its own role named "Admin", which must not be hijacked)
    var adminRole = await db.Set<Role>().IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Name == "Admin" && r.TenantId == "");
    if (adminRole == null)
    {
        adminRole = new Role { Name = "Admin" };
        db.Set<Role>().Add(adminRole);
        await db.SaveChangesAsync();
    }

    // Assign all permissions to admin role
    if (!await db.Set<RolePermission>().IgnoreQueryFilters().AnyAsync(rp => rp.RoleId == adminRole.Id))
    {
        var allPermissions = await db.Set<Permission>().ToListAsync();
        foreach (var permission in allPermissions)
        {
            db.Set<RolePermission>().Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permission.Id
            });
        }
        await db.SaveChangesAsync();
    }

    // Seed admin user
    var adminUser = await db.Set<User>().FirstOrDefaultAsync(u => u.Email == "admin@bpms.com");
    if (adminUser == null)
    {
        adminUser = new User
        {
            Email = "admin@bpms.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            FirstName = "System",
            LastName = "Admin",
            Status = UserStatus.Active,
            TenantId = "" // Empty string matches the default tenant context
        };
        db.Set<User>().Add(adminUser);
        await db.SaveChangesAsync();
    }
    else if (adminUser.TenantId != "")
    {
        // Fix existing user created without tenant
        adminUser.TenantId = "";
        await db.SaveChangesAsync();
    }

    // Assign admin role to admin user (check via IgnoreQueryFilters since Role has tenant filter)
    var isAssigned = await db.Set<UserRole>().IgnoreQueryFilters()
        .AnyAsync(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id);
    if (!isAssigned)
    {
        db.Set<UserRole>().Add(new UserRole
        {
            UserId = adminUser.Id,
            RoleId = adminRole.Id
        });
        await db.SaveChangesAsync();
    }

    // Backfill: provision tenant Owner roles for workspaces created before
    // the auto-provisioning fix existed (owners of those tenants currently
    // hold no permission claims and would get 403 on permission-gated endpoints).
    var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();
    var ownerMembers = await db.Set<global::BPMS.Modules.Tenancy.Entities.TenantMember>()
        .Where(tm => tm.Role == global::BPMS.Modules.Tenancy.Entities.TenantMemberRole.Owner)
        .ToListAsync();

    foreach (var owner in ownerMembers)
    {
        var tenantIdStr = owner.TenantId.ToString();

        var alreadyHasRole = await db.Set<UserRole>().IgnoreQueryFilters()
            .AnyAsync(ur => ur.UserId == owner.UserId && ur.Role.TenantId == tenantIdStr);
        if (alreadyHasRole)
            continue;

        // Set the tenant context so SaveChanges stamps the role with this tenant
        tenantService.SetTenant(tenantIdStr);

        var ownerRole = new Role { Name = "Owner", TenantId = tenantIdStr, CreatedBy = "system" };
        db.Set<Role>().Add(ownerRole);

        var allPermissions = await db.Set<Permission>().ToListAsync();
        foreach (var permission in allPermissions)
            db.Set<RolePermission>().Add(new RolePermission
            {
                RoleId = ownerRole.Id,
                PermissionId = permission.Id
            });

        db.Set<UserRole>().Add(new UserRole { UserId = owner.UserId, RoleId = ownerRole.Id });
        await db.SaveChangesAsync();
    }
}

Log.Information("Application started. Listening on {Urls}", app.Urls.Any() ? string.Join(", ", app.Urls) : "http://localhost:5171");
await app.RunAsync();