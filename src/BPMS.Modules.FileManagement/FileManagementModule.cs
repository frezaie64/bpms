using BPMS.Modules.FileManagement.Entities;
using BPMS.Modules.FileManagement.Models;
using BPMS.Modules.FileManagement.Services;
using BPMS.Shared.Persistence;
using BPMS.Shared.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BPMS.Modules.FileManagement;

public class FileManagementModule : IFileManagementModule
{
    private readonly BpmsDbContext _db;
    private readonly ITenantService _tenant;
    private readonly IFileStorageService _fileStorage;
    private readonly MinioOptions _minioOptions;
    private readonly FileValidationOptions _validationOptions;

    private static readonly HashSet<string> PreviewableContentTypes =
    [
        "image/jpeg", "image/png", "image/gif", "image/webp", "image/bmp",
        "application/pdf"
    ];

    public FileManagementModule(
        BpmsDbContext db,
        ITenantService tenant,
        IFileStorageService fileStorage,
        IOptions<MinioOptions> minioOptions,
        IOptions<FileValidationOptions> validationOptions)
    {
        _db = db;
        _tenant = tenant;
        _fileStorage = fileStorage;
        _minioOptions = minioOptions.Value;
        _validationOptions = validationOptions.Value;
    }

    public async Task<List<FileResponse>> GetFilesAsync(string? category, CancellationToken ct)
    {
        var query = _db.Set<FileItem>().AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(f => f.Category == category);

        var files = await query
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);

        return files.Select(MapFileResponse).ToList();
    }

    public async Task<FileResponse> GetFileByIdAsync(Guid id, CancellationToken ct)
    {
        var file = await _db.Set<FileItem>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"File with ID {id} not found.");

        return MapFileResponse(file);
    }

    public async Task<FileResponse> UploadFileAsync(IFormFile file, string category, CancellationToken ct)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(ext) || !_validationOptions.AllowedExtensions.Contains(ext))
            throw new InvalidOperationException($"File extension '{ext}' is not allowed.");

        if (file.Length > _validationOptions.MaxFileSizeBytes)
            throw new InvalidOperationException($"File size exceeds the maximum allowed size of {_validationOptions.MaxFileSizeBytes / 1024 / 1024} MB.");

        var objectName = $"{_tenant.TenantId}/{category}/{Guid.NewGuid():N}{ext}";
        var bucketName = _minioOptions.BucketName;

        await _fileStorage.EnsureBucketExistsAsync(bucketName, ct);

        using var stream = file.OpenReadStream();
        await _fileStorage.UploadAsync(objectName, bucketName, stream, file.ContentType, ct);

        var fileItem = new FileItem
        {
            TenantId = _tenant.TenantId,
            FileName = file.FileName,
            OriginalFileName = file.FileName,
            FileSize = file.Length,
            ContentType = file.ContentType,
            ObjectName = objectName,
            BucketName = bucketName,
            Category = category
        };

        _db.Set<FileItem>().Add(fileItem);
        await _db.SaveChangesAsync(ct);

        return MapFileResponse(fileItem);
    }

    public async Task<(Stream Content, string ContentType, string FileName)> DownloadFileAsync(Guid id, CancellationToken ct)
    {
        var file = await _db.Set<FileItem>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"File with ID {id} not found.");

        var content = await _fileStorage.DownloadAsync(file.ObjectName, file.BucketName, ct)
            ?? throw new InvalidOperationException("File content not found in storage.");

        return (content, file.ContentType, file.FileName);
    }

    public async Task<FileResponse> RenameFileAsync(Guid id, RenameFileRequest request, CancellationToken ct)
    {
        var file = await _db.Set<FileItem>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"File with ID {id} not found.");

        file.FileName = request.FileName;
        await _db.SaveChangesAsync(ct);

        return MapFileResponse(file);
    }

    public async Task DeleteFileAsync(Guid id, CancellationToken ct)
    {
        var file = await _db.Set<FileItem>().FindAsync([id, ct], ct)
            ?? throw new KeyNotFoundException($"File with ID {id} not found.");

        file.IsDeleted = true;
        await _db.SaveChangesAsync(ct);
    }

    private static FileResponse MapFileResponse(FileItem file)
    {
        return new FileResponse(
            file.Id,
            file.FileName,
            file.OriginalFileName,
            file.FileSize,
            file.ContentType,
            file.Category,
            file.CreatedBy,
            file.CreatedAt,
            file.UpdatedAt
        );
    }
}