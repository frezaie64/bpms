using BPMS.Modules.Forms.Entities;
using BPMS.Modules.Forms.Models;
using BPMS.Shared.Abstractions;
using BPMS.Shared.Persistence;
using BPMS.Shared.Services;
using Microsoft.EntityFrameworkCore;

namespace BPMS.Modules.Forms;

public class FormsModule : IFormsModule
{
    private readonly BpmsDbContext _db;
    private readonly ITenantService _tenant;

    public FormsModule(BpmsDbContext db, ITenantService tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<FormResponse> CreateFormAsync(CreateFormRequest request, CancellationToken ct)
    {
        if (request.CategoryId.HasValue)
        {
            var categoryExists = await _db.Set<FormCategory>()
                .AnyAsync(c => c.Id == request.CategoryId.Value, ct);
            if (!categoryExists)
                throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found.");
        }

        var form = new Entities.Form
        {
            TenantId = _tenant.TenantId,
            Name = request.Name,
            Description = request.Description,
            CategoryId = request.CategoryId,
            JsonDefinition = request.JsonDefinition ?? "{}"
        };

        _db.Set<Entities.Form>().Add(form);
        await _db.SaveChangesAsync(ct);

        return MapFormResponse(form);
    }

    public async Task<FormResponse> UpdateFormAsync(Guid id, UpdateFormRequest request, CancellationToken ct)
    {
        var form = await _db.Set<Entities.Form>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"Form with ID {id} not found.");

        if (request.CategoryId.HasValue)
        {
            var categoryExists = await _db.Set<FormCategory>()
                .AnyAsync(c => c.Id == request.CategoryId.Value, ct);
            if (!categoryExists)
                throw new KeyNotFoundException($"Category with ID {request.CategoryId} not found.");
        }

        if (form.Status == FormStatus.Published)
        {
            var version = new FormVersion
            {
                FormId = form.Id,
                VersionNumber = form.CurrentVersion,
                JsonDefinition = form.JsonDefinition,
                Status = form.Status,
                CreatedAt = DateTime.UtcNow
            };
            _db.Set<FormVersion>().Add(version);

            form.CurrentVersion++;
            form.Status = FormStatus.Draft;
        }

        form.Name = request.Name;
        form.Description = request.Description;
        form.CategoryId = request.CategoryId;
        form.JsonDefinition = request.JsonDefinition ?? form.JsonDefinition;

        await _db.SaveChangesAsync(ct);

        return MapFormResponse(form);
    }

    public async Task DeleteFormAsync(Guid id, CancellationToken ct)
    {
        var form = await _db.Set<Entities.Form>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"Form with ID {id} not found.");

        form.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task PublishFormAsync(Guid id, CancellationToken ct)
    {
        var form = await _db.Set<Entities.Form>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"Form with ID {id} not found.");

        if (form.Status == FormStatus.Published)
            return;

        var version = new FormVersion
        {
            FormId = form.Id,
            VersionNumber = form.CurrentVersion,
            JsonDefinition = form.JsonDefinition,
            Status = FormStatus.Published,
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<FormVersion>().Add(version);

        form.Status = FormStatus.Published;
        await _db.SaveChangesAsync(ct);
    }

    public async Task ArchiveFormAsync(Guid id, CancellationToken ct)
    {
        var form = await _db.Set<Entities.Form>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"Form with ID {id} not found.");

        form.Status = FormStatus.Archived;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<FormDetailResponse> GetFormByIdAsync(Guid id, CancellationToken ct)
    {
        var form = await _db.Set<Entities.Form>()
            .Include(f => f.Category)
            .Include(f => f.Versions.OrderByDescending(v => v.VersionNumber))
            .FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new KeyNotFoundException($"Form with ID {id} not found.");

        return MapFormDetailResponse(form);
    }

    public async Task<List<FormResponse>> GetFormsAsync(CancellationToken ct)
    {
        var forms = await _db.Set<Entities.Form>()
            .Include(f => f.Category)
            .OrderByDescending(f => f.UpdatedAt ?? f.CreatedAt)
            .ToListAsync(ct);

        return forms.Select(MapFormResponse).ToList();
    }

    public async Task<FormDetailResponse> DuplicateFormAsync(Guid id, CancellationToken ct)
    {
        var source = await _db.Set<Entities.Form>()
            .Include(f => f.Category)
            .FirstOrDefaultAsync(f => f.Id == id, ct)
            ?? throw new KeyNotFoundException($"Form with ID {id} not found.");

        var duplicate = new Entities.Form
        {
            TenantId = _tenant.TenantId,
            Name = $"Copy of {source.Name}",
            Description = source.Description,
            CategoryId = source.CategoryId,
            Status = FormStatus.Draft,
            CurrentVersion = 1,
            JsonDefinition = source.JsonDefinition
        };

        _db.Set<Entities.Form>().Add(duplicate);
        await _db.SaveChangesAsync(ct);

        return MapFormDetailResponse(duplicate);
    }

    public async Task<FormDetailResponse> PreviewFormAsync(Guid id, CancellationToken ct)
    {
        return await GetFormByIdAsync(id, ct);
    }

    public async Task<FormCategoryResponse> CreateCategoryAsync(CreateFormCategoryRequest request, CancellationToken ct)
    {
        var category = new FormCategory
        {
            TenantId = _tenant.TenantId,
            Name = request.Name,
            Description = request.Description
        };

        _db.Set<FormCategory>().Add(category);
        await _db.SaveChangesAsync(ct);

        return MapCategoryResponse(category);
    }

    public async Task<FormCategoryResponse> UpdateCategoryAsync(Guid id, UpdateFormCategoryRequest request, CancellationToken ct)
    {
        var category = await _db.Set<FormCategory>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"Category with ID {id} not found.");

        category.Name = request.Name;
        category.Description = request.Description;

        await _db.SaveChangesAsync(ct);

        return MapCategoryResponse(category);
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken ct)
    {
        var category = await _db.Set<FormCategory>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"Category with ID {id} not found.");

        category.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<List<FormCategoryResponse>> GetCategoriesAsync(CancellationToken ct)
    {
        var categories = await _db.Set<FormCategory>()
            .OrderBy(c => c.Name)
            .ToListAsync(ct);

        return categories.Select(MapCategoryResponse).ToList();
    }

    private static FormResponse MapFormResponse(Entities.Form form)
    {
        return new FormResponse(
            form.Id,
            form.Name,
            form.Description,
            form.CategoryId,
            form.Category?.Name,
            form.Status,
            form.CurrentVersion,
            form.CreatedAt,
            form.UpdatedAt
        );
    }

    private static FormDetailResponse MapFormDetailResponse(Entities.Form form)
    {
        return new FormDetailResponse(
            form.Id,
            form.Name,
            form.Description,
            form.CategoryId,
            form.Category?.Name,
            form.Status,
            form.CurrentVersion,
            form.JsonDefinition,
            form.CreatedAt,
            form.CreatedBy,
            form.UpdatedAt,
            form.UpdatedBy,
            form.Versions.Select(v => new FormVersionResponse(
                v.Id,
                v.VersionNumber,
                v.Status,
                v.JsonDefinition,
                v.CreatedAt,
                v.Notes
            )).ToList()
        );
    }

    private static FormCategoryResponse MapCategoryResponse(FormCategory category)
    {
        return new FormCategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.CreatedAt,
            category.UpdatedAt
        );
    }
}