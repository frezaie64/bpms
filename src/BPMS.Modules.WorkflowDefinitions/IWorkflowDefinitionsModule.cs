using BPMS.Modules.WorkflowDefinitions.Models;

namespace BPMS.Modules.WorkflowDefinitions;

public interface IWorkflowDefinitionsModule
{
    Task<WorkflowResponse> CreateWorkflowAsync(CreateWorkflowRequest request, CancellationToken ct = default);
    Task<WorkflowResponse> UpdateWorkflowAsync(Guid id, UpdateWorkflowRequest request, CancellationToken ct = default);
    Task DeleteWorkflowAsync(Guid id, CancellationToken ct = default);
    Task PublishWorkflowAsync(Guid id, CancellationToken ct = default);
    Task ArchiveWorkflowAsync(Guid id, CancellationToken ct = default);
    Task<WorkflowDetailResponse> GetWorkflowByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<WorkflowResponse>> GetWorkflowsAsync(CancellationToken ct = default);
    Task<WorkflowDetailResponse> DuplicateWorkflowAsync(Guid id, CancellationToken ct = default);
    Task<string> GetWorkflowDefinitionAsync(Guid id, CancellationToken ct = default);

    Task<WorkflowSubscriptionResponse> SubscribeUserAsync(Guid workflowId, Guid userId, CancellationToken ct = default);
    Task UnsubscribeUserAsync(Guid id, CancellationToken ct = default);
    Task<List<WorkflowSubscriptionResponse>> GetSubscribersAsync(Guid workflowId, CancellationToken ct = default);
    Task<List<CatalogWorkflowResponse>> GetAvailableWorkflowsAsync(string userId, CancellationToken ct = default);
    Task<bool> IsUserSubscribedAsync(Guid workflowId, string userId, CancellationToken ct = default);

    Task<Models.WorkflowCategoryResponse> CreateWorkflowCategoryAsync(Models.CreateWorkflowCategoryRequest request, CancellationToken ct = default);
    Task<Models.WorkflowCategoryResponse> UpdateWorkflowCategoryAsync(Guid id, Models.UpdateWorkflowCategoryRequest request, CancellationToken ct = default);
    Task DeleteWorkflowCategoryAsync(Guid id, CancellationToken ct = default);
    Task<List<Models.WorkflowCategoryResponse>> GetWorkflowCategoriesAsync(CancellationToken ct = default);
}