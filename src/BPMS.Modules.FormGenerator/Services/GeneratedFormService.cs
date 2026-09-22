using BPMS.Modules.FormGenerator.Entities;
using BPMS.Modules.FormGenerator.Models;
using BPMS.Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BPMS.Modules.FormGenerator.Services;

public sealed class GeneratedFormService
{
    private readonly BpmsDbContext _db;
    private readonly IFormGenerator _generator;
    private readonly AiGenerationState _generationState;

    public GeneratedFormService(BpmsDbContext db, IFormGenerator generator, AiGenerationState generationState)
    {
        _db = db;
        _generator = generator;
        _generationState = generationState;
    }

    public async Task<GenerateFormResponse> GenerateFormDraftAsync(string prompt, CancellationToken cancellationToken = default)
    {
        _generationState.Reset();

        GenerateFormResponse form;
        try
        {
            form = await _generator.GenerateAsync(prompt, cancellationToken);
        }
        catch (Exception ex)
        {
            form = DeterministicFormGenerator.Generate(prompt);
            _generationState.SetFallback(DeterministicFormGenerator.ToJson(form), ex.Message);
        }

        return FormValidator.SanitizeAndValidateForm(form);
    }

    public async Task<GenerateFormResponse> AcceptAndSaveFormAsync(GenerateFormResponse formDto, string creatorUserId, CancellationToken cancellationToken = default)
    {
        var normalized = FormValidator.SanitizeAndValidateForm(formDto);

        var form = new GeneratedForm
        {
            Name = normalized.Name,
            Description = normalized.Description,
            Prompt = normalized.Prompt,
            CreatorUserId = creatorUserId,
            StartDate = normalized.StartDate,
            ExpireDate = normalized.ExpireDate,
            Fields = normalized.Fields.Select(f => new FormField
            {
                Key = f.Key,
                Label = f.Label,
                Type = f.Type,
                Required = f.Required,
                Placeholder = f.Placeholder,
                Options = f.Options,
                OrderIndex = f.OrderIndex
            }).ToList()
        };

        _db.Set<GeneratedForm>().Add(form);
        await _db.SaveChangesAsync(cancellationToken);

        return MapToResponse(form);
    }

    public async Task<FormDetailResponse?> GetFormAsync(Guid formId, CancellationToken cancellationToken = default)
    {
        var form = await _db.Set<GeneratedForm>()
            .Include(f => f.Fields.OrderBy(fld => fld.OrderIndex))
            .FirstOrDefaultAsync(f => f.Id == formId, cancellationToken);

        if (form is null) return null;

        return new FormDetailResponse
        {
            Id = form.Id,
            Name = form.Name,
            Description = form.Description,
            Prompt = form.Prompt,
            CreatorUserId = form.CreatorUserId,
            StartDate = form.StartDate,
            ExpireDate = form.ExpireDate,
            CreatedAt = form.CreatedAt,
            Fields = form.Fields.Select(f => new FormFieldDto
            {
                Id = f.Id,
                Key = f.Key,
                Label = f.Label,
                Type = f.Type,
                Required = f.Required,
                Placeholder = f.Placeholder,
                Options = f.Options,
                OrderIndex = f.OrderIndex
            }).ToList()
        };
    }

    public async Task<SubmitFormResponse> SubmitFormAsync(Guid formId, string submittedBy, Dictionary<string, object?> data, CancellationToken cancellationToken = default)
    {
        var form = await _db.Set<GeneratedForm>()
            .Include(f => f.Fields)
            .FirstOrDefaultAsync(f => f.Id == formId, cancellationToken)
            ?? throw new KeyNotFoundException("form not found");

        FormValidator.ValidateFormAvailability(form.StartDate, form.ExpireDate);

        var fieldDtos = form.Fields.Select(f => new FormFieldDto
        {
            Key = f.Key,
            Label = f.Label,
            Type = f.Type,
            Required = f.Required,
            Placeholder = f.Placeholder,
            Options = f.Options,
            OrderIndex = f.OrderIndex
        }).ToList();

        FormValidator.ValidateSubmission(fieldDtos, data);

        var submission = new FormSubmission
        {
            FormId = formId,
            SubmittedBy = submittedBy,
            Data = data,
            SubmittedAt = DateTime.UtcNow
        };

        _db.Set<FormSubmission>().Add(submission);
        await _db.SaveChangesAsync(cancellationToken);

        return new SubmitFormResponse
        {
            Id = submission.Id,
            FormId = submission.FormId,
            SubmittedBy = submission.SubmittedBy,
            Data = submission.Data,
            SubmittedAt = submission.SubmittedAt
        };
    }

    public async Task<SubmitFormResponse?> GetSubmissionAsync(Guid formId, Guid submissionId, CancellationToken cancellationToken = default)
    {
        var submission = await _db.Set<FormSubmission>()
            .FirstOrDefaultAsync(s => s.Id == submissionId && s.FormId == formId, cancellationToken);

        if (submission is null) return null;

        return new SubmitFormResponse
        {
            Id = submission.Id,
            FormId = submission.FormId,
            SubmittedBy = submission.SubmittedBy,
            Data = submission.Data,
            SubmittedAt = submission.SubmittedAt
        };
    }

    public async Task BindFormToTaskAsync(string taskId, Guid formId, CancellationToken cancellationToken = default)
    {
        var form = await _db.Set<GeneratedForm>().FindAsync([formId, cancellationToken], cancellationToken);
        if (form is null)
            throw new KeyNotFoundException("form not found");

        var binding = new TaskFormBinding
        {
            TaskId = taskId,
            FormId = formId,
            CreatedAt = DateTime.UtcNow
        };

        // Upsert: remove existing if any, then add
        var existing = await _db.Set<TaskFormBinding>().FindAsync([taskId, cancellationToken], cancellationToken);
        if (existing is not null)
            _db.Set<TaskFormBinding>().Remove(existing);

        _db.Set<TaskFormBinding>().Add(binding);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<Guid?> GetFormIdForTaskAsync(string taskId, CancellationToken cancellationToken = default)
    {
        var binding = await _db.Set<TaskFormBinding>()
            .FirstOrDefaultAsync(t => t.TaskId == taskId, cancellationToken);

        return binding?.FormId;
    }

    private static GenerateFormResponse MapToResponse(GeneratedForm form)
    {
        return new GenerateFormResponse
        {
            Id = form.Id,
            Name = form.Name,
            Description = form.Description,
            Prompt = form.Prompt,
            CreatorUserId = form.CreatorUserId,
            StartDate = form.StartDate,
            ExpireDate = form.ExpireDate,
            CreatedAt = form.CreatedAt,
            Fields = form.Fields.OrderBy(f => f.OrderIndex).Select(f => new FormFieldDto
            {
                Id = f.Id,
                Key = f.Key,
                Label = f.Label,
                Type = f.Type,
                Required = f.Required,
                Placeholder = f.Placeholder,
                Options = f.Options,
                OrderIndex = f.OrderIndex
            }).ToList()
        };
    }
}