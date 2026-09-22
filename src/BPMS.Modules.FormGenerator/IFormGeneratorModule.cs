using BPMS.Modules.FormGenerator.Models;

namespace BPMS.Modules.FormGenerator;

public interface IFormGeneratorModule
{
    Task<GenerateFormResponse> GenerateFormDraftAsync(string prompt, CancellationToken ct = default);
    Task<GenerateFormResponse> AcceptAndSaveFormAsync(GenerateFormResponse formDto, string creatorUserId, CancellationToken ct = default);
    Task<FormDetailResponse?> GetFormAsync(Guid formId, CancellationToken ct = default);
    Task<SubmitFormResponse> SubmitFormAsync(Guid formId, string submittedBy, Dictionary<string, object?> data, CancellationToken ct = default);
    Task<SubmitFormResponse?> GetSubmissionAsync(Guid formId, Guid submissionId, CancellationToken ct = default);
    Task BindFormToTaskAsync(string taskId, Guid formId, CancellationToken ct = default);
    Task<Guid?> GetFormIdForTaskAsync(string taskId, CancellationToken ct = default);
}