namespace BPMS.Modules.Forms;

public interface IFormsModule
{
    Task<Models.FormResponse> CreateFormAsync(Models.CreateFormRequest request, CancellationToken ct = default);
    Task<Models.FormResponse> UpdateFormAsync(Guid id, Models.UpdateFormRequest request, CancellationToken ct = default);
    Task DeleteFormAsync(Guid id, CancellationToken ct = default);
    Task PublishFormAsync(Guid id, CancellationToken ct = default);
    Task ArchiveFormAsync(Guid id, CancellationToken ct = default);
    Task<Models.FormDetailResponse> GetFormByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<Models.FormResponse>> GetFormsAsync(CancellationToken ct = default);
    Task<Models.FormDetailResponse> DuplicateFormAsync(Guid id, CancellationToken ct = default);
    Task<Models.FormDetailResponse> PreviewFormAsync(Guid id, CancellationToken ct = default);

    // Categories
    Task<Models.FormCategoryResponse> CreateCategoryAsync(Models.CreateFormCategoryRequest request, CancellationToken ct = default);
    Task<Models.FormCategoryResponse> UpdateCategoryAsync(Guid id, Models.UpdateFormCategoryRequest request, CancellationToken ct = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken ct = default);
    Task<List<Models.FormCategoryResponse>> GetCategoriesAsync(CancellationToken ct = default);
}