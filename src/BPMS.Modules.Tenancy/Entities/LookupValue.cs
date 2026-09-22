using BPMS.Shared.Abstractions;

namespace BPMS.Modules.Tenancy.Entities;

public class LookupValue : BaseEntity, ITenantEntity, IAuditableEntity
{
    public string TenantId { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}