using BPMS.Modules.Tenancy.Entities;

namespace BPMS.Modules.Tenancy.Models;

public record InviteByEmailRequest(
    string Email,
    TenantMemberRole Role = TenantMemberRole.Member
);

public record AcceptInvitationRequest(
    string Token
);

public record InvitationResponse(
    Guid Id,
    string Email,
    TenantMemberRole Role,
    InvitationStatus Status,
    DateTime ExpiresAt,
    DateTime? AcceptedAt,
    DateTime CreatedAt
);

public record InvitationAcceptResponse(
    Guid TenantId,
    Guid UserId,
    TenantMemberRole Role
);
