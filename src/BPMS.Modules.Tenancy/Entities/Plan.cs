using BPMS.Shared.Abstractions;

namespace BPMS.Modules.Tenancy.Entities;

public class Plan : BaseEntity, IAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int MaxUsers { get; set; } = 1;
    public int MaxWorkflows { get; set; } = 5;
    public int MaxAiFormGenerations { get; set; } = 10;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}