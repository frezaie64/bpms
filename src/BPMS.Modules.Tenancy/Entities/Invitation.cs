using BPMS.Shared.Abstractions;

namespace BPMS.Modules.Tenancy.Entities;

public enum InvitationStatus
{
    Pending = 0,
    Accepted = 1,
    Expired = 2,
    Cancelled = 3
}

public class Invitation : BaseEntity, IAuditableEntity
{
    public Guid TenantId { get; set; }
    public string Email { get; set; } = string.Empty;
    public TenantMemberRole Role { get; set; } = TenantMemberRole.Member;
    public Guid InvitedByUserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
