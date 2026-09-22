using BPMS.Shared.Abstractions;

namespace BPMS.Modules.FileManagement.Entities;

public class FileItem : BaseEntity, ITenantEntity, IAuditableEntity
{
    public string TenantId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string Category { get; set; } = "general";
    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}