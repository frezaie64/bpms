using BPMS.Modules.FileManagement.Models;
using Microsoft.AspNetCore.Http;

namespace BPMS.Modules.FileManagement;

public interface IFileManagementModule
{
    Task<List<FileResponse>> GetFilesAsync(string? category, CancellationToken ct = default);
    Task<FileResponse> GetFileByIdAsync(Guid id, CancellationToken ct = default);
    Task<FileResponse> UploadFileAsync(IFormFile file, string category, CancellationToken ct = default);
    Task<(Stream Content, string ContentType, string FileName)> DownloadFileAsync(Guid id, CancellationToken ct = default);
    Task<FileResponse> RenameFileAsync(Guid id, RenameFileRequest request, CancellationToken ct = default);
    Task DeleteFileAsync(Guid id, CancellationToken ct = default);
}