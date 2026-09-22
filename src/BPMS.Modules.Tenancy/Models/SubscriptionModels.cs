using BPMS.Modules.Tenancy.Entities;

namespace BPMS.Modules.Tenancy.Models;

public record CreateSubscriptionRequest(Guid PlanId);

public record SubscriptionResponse(
    Guid Id,
    Guid UserId,
    Guid PlanId,
    SubscriptionStatus Status,
    DateTime StartDate,
    DateTime? EndDate
);

public record PayPaymentResponse(
    Guid Id,
    PaymentStatus Status,
    string? TransactionId,
    decimal Amount
);

public record SubscriptionStatusResponse(
    bool HasActiveSubscription,
    Guid? SubscriptionId,
    Guid? PlanId,
    string? PlanName,
    DateTime? EndDate
);
