namespace BPMS.Modules.FileManagement.Models;

public record RenameFileRequest(
    string FileName
);

public record FileResponse(
    Guid Id,
    string FileName,
    string OriginalFileName,
    long FileSize,
    string ContentType,
    string Category,
    string CreatedBy,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);