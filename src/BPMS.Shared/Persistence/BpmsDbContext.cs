using System.Reflection;
using BPMS.Shared.Abstractions;
using BPMS.Shared.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BPMS.Shared.Persistence;

public class BpmsDbContext : DbContext
{
    private readonly ITenantService _tenantService;

    public BpmsDbContext(DbContextOptions<BpmsDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _tenantService = tenantService;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        var assemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name?.StartsWith("BPMS.Modules.") == true)
            .ToList();

        foreach (var assembly in assemblies)
            modelBuilder.ApplyConfigurationsFromAssembly(assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (entityType.ClrType.Name == "User")
                continue;

            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(BpmsDbContext).GetMethod(nameof(ApplyTenantFilter), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
                var generic = method.MakeGenericMethod(entityType.ClrType);
                generic.Invoke(this, [modelBuilder]);
            }
        }

    }

    private void ApplyTenantFilter<T>(ModelBuilder modelBuilder) where T : class, ITenantEntity
    {
        modelBuilder.Entity<T>().HasQueryFilter(e => e.TenantId == _tenantService.TenantId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<ITenantEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property("TenantId").CurrentValue = _tenantService.TenantId;
            }
        }

        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.GetType().GetProperty("CreatedAt")?.SetValue(entry.Entity, DateTime.UtcNow);
                entry.Entity.GetType().GetProperty("CreatedBy")?.SetValue(entry.Entity, _tenantService.UserId ?? "system");
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.GetType().GetProperty("UpdatedAt")?.SetValue(entry.Entity, DateTime.UtcNow);
                entry.Entity.GetType().GetProperty("UpdatedBy")?.SetValue(entry.Entity, _tenantService.UserId ?? "system");
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}