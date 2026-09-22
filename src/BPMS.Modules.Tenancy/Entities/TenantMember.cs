namespace BPMS.Modules.Tenancy.Entities;

public enum TenantMemberRole
{
    Owner = 0,
    Admin = 1,
    Member = 2
}

public class TenantMember
{
    public Guid TenantId { get; set; }
    public Guid UserId { get; set; }
    public TenantMemberRole Role { get; set; } = TenantMemberRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public Tenant Tenant { get; set; } = null!;
}