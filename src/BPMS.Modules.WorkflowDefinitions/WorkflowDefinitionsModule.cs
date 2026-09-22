using BPMS.Modules.WorkflowDefinitions.Entities;
using BPMS.Modules.WorkflowDefinitions.Models;
using BPMS.Shared.Abstractions;
using BPMS.Shared.Persistence;
using BPMS.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BPMS.Modules.WorkflowDefinitions;

public class WorkflowDefinitionsModule : IWorkflowDefinitionsModule
{
    private readonly BpmsDbContext _db;
    private readonly ITenantService _tenant;

    public WorkflowDefinitionsModule(BpmsDbContext db, ITenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<WorkflowResponse> CreateWorkflowAsync(CreateWorkflowRequest request, CancellationToken ct)
    {
        var workflow = new Entities.Workflow
        {
            TenantId = _tenant.TenantId,
            Name = request.Name,
            Description = request.Description,
            JsonDefinition = request.JsonDefinition ?? "{}"
        };

        _db.Set<Entities.Workflow>().Add(workflow);
        await _db.SaveChangesAsync(ct);

        return MapWorkflowResponse(workflow);
    }

    public async Task<WorkflowResponse> UpdateWorkflowAsync(Guid id, UpdateWorkflowRequest request, CancellationToken ct)
    {
        var workflow = await _db.Set<Entities.Workflow>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"Workflow with ID {id} not found.");

        if (workflow.Status == WorkflowStatus.Published)
        {
            var version = new WorkflowVersion
            {
                WorkflowId = workflow.Id,
                VersionNumber = workflow.CurrentVersion,
                JsonDefinition = workflow.JsonDefinition,
                Status = workflow.Status,
                CreatedAt = DateTime.UtcNow
            };
            _db.Set<WorkflowVersion>().Add(version);

            workflow.CurrentVersion++;
            workflow.Status = WorkflowStatus.Draft;
        }

        workflow.Name = request.Name;
        workflow.Description = request.Description;
        workflow.JsonDefinition = request.JsonDefinition ?? workflow.JsonDefinition;

        await _db.SaveChangesAsync(ct);

        return MapWorkflowResponse(workflow);
    }

    public async Task DeleteWorkflowAsync(Guid id, CancellationToken ct)
    {
        var workflow = await _db.Set<Entities.Workflow>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"Workflow with ID {id} not found.");

        workflow.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task PublishWorkflowAsync(Guid id, CancellationToken ct)
    {
        var workflow = await _db.Set<Entities.Workflow>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"Workflow with ID {id} not found.");

        if (workflow.Status == WorkflowStatus.Published)
            return;

        var version = new WorkflowVersion
        {
            WorkflowId = workflow.Id,
            VersionNumber = workflow.CurrentVersion,
            JsonDefinition = workflow.JsonDefinition,
            Status = WorkflowStatus.Published,
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<WorkflowVersion>().Add(version);

        workflow.Status = WorkflowStatus.Published;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ArchiveWorkflowAsync(Guid id, CancellationToken ct)
    {
        var workflow = await _db.Set<Entities.Workflow>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"Workflow with ID {id} not found.");

        workflow.Status = WorkflowStatus.Archived;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<WorkflowDetailResponse> GetWorkflowByIdAsync(Guid id, CancellationToken ct)
    {
        var workflow = await _db.Set<Entities.Workflow>()
            .Include(w => w.Versions.OrderByDescending(v => v.VersionNumber))
            .FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new KeyNotFoundException($"Workflow with ID {id} not found.");

        return MapWorkflowDetailResponse(workflow);
    }

    public async Task<List<WorkflowResponse>> GetWorkflowsAsync(CancellationToken ct)
    {
        var workflows = await _db.Set<Entities.Workflow>()
            .OrderByDescending(w => w.UpdatedAt ?? w.CreatedAt)
            .ToListAsync(ct);

        return workflows.Select(MapWorkflowResponse).ToList();
    }

    public async Task<WorkflowDetailResponse> DuplicateWorkflowAsync(Guid id, CancellationToken ct)
    {
        var source = await _db.Set<Entities.Workflow>()
            .FirstOrDefaultAsync(w => w.Id == id, ct)
            ?? throw new KeyNotFoundException($"Workflow with ID {id} not found.");

        var duplicate = new Entities.Workflow
        {
            TenantId = _tenant.TenantId,
            Name = $"Copy of {source.Name}",
            Description = source.Description,
            Status = WorkflowStatus.Draft,
            CurrentVersion = 1,
            JsonDefinition = source.JsonDefinition
        };

        _db.Set<Entities.Workflow>().Add(duplicate);
        await _db.SaveChangesAsync(ct);

        return MapWorkflowDetailResponse(duplicate);
    }

    public async Task<string> GetWorkflowDefinitionAsync(Guid id, CancellationToken ct)
    {
        var workflow = await _db.Set<Entities.Workflow>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"Workflow with ID {id} not found.");

        return workflow.JsonDefinition;
    }

    public async Task<WorkflowSubscriptionResponse> SubscribeUserAsync(Guid workflowId, Guid userId, CancellationToken ct)
    {
        var workflow = await _db.Set<Entities.Workflow>().FindAsync([workflowId, ct], ct)
            ?? throw new KeyNotFoundException("Workflow not found.");

        if (workflow.Status != WorkflowStatus.Published)
            throw new InvalidOperationException("Only published workflows can have subscribers.");

        var existing = await _db.Set<WorkflowSubscription>()
            .FirstOrDefaultAsync(s => s.WorkflowId == workflowId && s.UserId == userId.ToString(), ct);

        if (existing != null)
            throw new InvalidOperationException("User is already subscribed to this workflow.");

        var subscription = new WorkflowSubscription
        {
            TenantId = _tenant.TenantId,
            WorkflowId = workflowId,
            UserId = userId.ToString()
        };

        _db.Set<WorkflowSubscription>().Add(subscription);
        await _db.SaveChangesAsync(ct);

        return new WorkflowSubscriptionResponse(
            subscription.Id,
            subscription.WorkflowId,
            Guid.Parse(subscription.UserId)
        );
    }

    public async Task UnsubscribeUserAsync(Guid id, CancellationToken ct)
    {
        var subscription = await _db.Set<WorkflowSubscription>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException("Subscription not found.");

        _db.Set<WorkflowSubscription>().Remove(subscription);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<WorkflowSubscriptionResponse>> GetSubscribersAsync(Guid workflowId, CancellationToken ct)
    {
        var subscriptions = await _db.Set<WorkflowSubscription>()
            .Where(s => s.WorkflowId == workflowId)
            .ToListAsync(ct);

        return subscriptions.Select(s => new WorkflowSubscriptionResponse(
            s.Id,
            s.WorkflowId,
            Guid.Parse(s.UserId)
        )).ToList();
    }

    public async Task<List<CatalogWorkflowResponse>> GetAvailableWorkflowsAsync(string userId, CancellationToken ct)
    {
        var subscribedIds = await _db.Set<WorkflowSubscription>()
            .Where(s => s.UserId == userId)
            .Select(s => s.WorkflowId)
            .ToListAsync(ct);

        var workflows = await _db.Set<Entities.Workflow>()
            .Where(w => w.Status == WorkflowStatus.Published && subscribedIds.Contains(w.Id))
            .OrderByDescending(w => w.UpdatedAt ?? w.CreatedAt)
            .ToListAsync(ct);

        return workflows.Select(w => new CatalogWorkflowResponse(
            w.Id,
            w.Name,
            w.Description,
            w.CurrentVersion,
            w.CreatedAt
        )).ToList();
    }

    public async Task<bool> IsUserSubscribedAsync(Guid workflowId, string userId, CancellationToken ct)
    {
        return await _db.Set<WorkflowSubscription>()
            .AnyAsync(s => s.WorkflowId == workflowId && s.UserId == userId, ct);
    }

    public async Task<Models.WorkflowCategoryResponse> CreateWorkflowCategoryAsync(Models.CreateWorkflowCategoryRequest request, CancellationToken ct)
    {
        var category = new WorkflowCategory
        {
            TenantId = _tenant.TenantId,
            Name = request.Name,
            Description = request.Description
        };
        _db.Set<WorkflowCategory>().Add(category);
        await _db.SaveChangesAsync(ct);
        return MapWorkflowCategoryResponse(category);
    }

    public async Task<Models.WorkflowCategoryResponse> UpdateWorkflowCategoryAsync(Guid id, Models.UpdateWorkflowCategoryRequest request, CancellationToken ct)
    {
        var category = await _db.Set<WorkflowCategory>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"Workflow category with ID {id} not found.");
        category.Name = request.Name;
        category.Description = request.Description;
        await _db.SaveChangesAsync(ct);
        return MapWorkflowCategoryResponse(category);
    }

    public async Task DeleteWorkflowCategoryAsync(Guid id, CancellationToken ct)
    {
        var category = await _db.Set<WorkflowCategory>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"Workflow category with ID {id} not found.");
        category.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<Models.WorkflowCategoryResponse>> GetWorkflowCategoriesAsync(CancellationToken ct)
    {
        var categories = await _db.Set<WorkflowCategory>()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);
        return categories.Select(MapWorkflowCategoryResponse).ToList();
    }

    private static Models.WorkflowCategoryResponse MapWorkflowCategoryResponse(WorkflowCategory c)
    {
        return new Models.WorkflowCategoryResponse(c.Id, c.Name, c.Description, c.CreatedAt, c.UpdatedAt);
    }

    private static WorkflowResponse MapWorkflowResponse(Entities.Workflow workflow)
    {
        return new WorkflowResponse(
            workflow.Id,
            workflow.Name,
            workflow.Description,
            workflow.Status,
            workflow.CurrentVersion,
            workflow.CreatedAt,
            workflow.UpdatedAt
        );
    }

    private static WorkflowDetailResponse MapWorkflowDetailResponse(Entities.Workflow workflow)
    {
        return new WorkflowDetailResponse(
            workflow.Id,
            workflow.Name,
            workflow.Description,
            workflow.Status,
            workflow.CurrentVersion,
            workflow.JsonDefinition,
            workflow.CreatedAt,
            workflow.CreatedBy,
            workflow.UpdatedAt,
            workflow.UpdatedBy,
            workflow.Versions.Select(v => new WorkflowVersionResponse(
                v.Id,
                v.VersionNumber,
                v.Status,
                v.JsonDefinition,
                v.CreatedAt,
                v.Notes
            )).ToList()
        );
    }
}