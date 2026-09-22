using BPMS.Modules.FormGenerator.Models;
using BPMS.Modules.FormGenerator.Services;

namespace BPMS.Modules.FormGenerator;

public class FormGeneratorModule : IFormGeneratorModule
{
    private readonly GeneratedFormService _formService;

    public FormGeneratorModule(GeneratedFormService formService)
    {
        _formService = formService;
    }

    public Task<GenerateFormResponse> GenerateFormDraftAsync(string prompt, CancellationToken ct = default)
        => _formService.GenerateFormDraftAsync(prompt, ct);

    public Task<GenerateFormResponse> AcceptAndSaveFormAsync(GenerateFormResponse formDto, string creatorUserId, CancellationToken ct = default)
        => _formService.AcceptAndSaveFormAsync(formDto, creatorUserId, ct);

    public Task<FormDetailResponse?> GetFormAsync(Guid formId, CancellationToken ct = default)
        => _formService.GetFormAsync(formId, ct);

    public Task<SubmitFormResponse> SubmitFormAsync(Guid formId, string submittedBy, Dictionary<string, object?> data, CancellationToken ct = default)
        => _formService.SubmitFormAsync(formId, submittedBy, data, ct);

    public Task<SubmitFormResponse?> GetSubmissionAsync(Guid formId, Guid submissionId, CancellationToken ct = default)
        => _formService.GetSubmissionAsync(formId, submissionId, ct);

    public Task BindFormToTaskAsync(string taskId, Guid formId, CancellationToken ct = default)
        => _formService.BindFormToTaskAsync(taskId, formId, ct);

    public Task<Guid?> GetFormIdForTaskAsync(string taskId, CancellationToken ct = default)
        => _formService.GetFormIdForTaskAsync(taskId, ct);
}