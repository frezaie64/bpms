using BPMS.Shared.Abstractions;

namespace BPMS.Modules.Tenancy.Entities;

public class TenantSetting : BaseEntity, ITenantEntity, IAuditableEntity
{
    public string TenantId { get; set; } = string.Empty;
    public string? TenantName { get; set; }
    public string? Logo { get; set; }
    public string? TimeZone { get; set; }
    public string? DateFormat { get; set; }
    public string? Language { get; set; }
    public int? DefaultFileSizeLimit { get; set; }
    public int? DefaultPageSize { get; set; }

    public string? SmtpHost { get; set; }
    public int? SmtpPort { get; set; }
    public string? SmtpUsername { get; set; }
    public string? SmtpPassword { get; set; }
    public string? SenderName { get; set; }
    public string? SenderEmail { get; set; }

    public string? SmsProviderName { get; set; }
    public string? SmsApiUrl { get; set; }
    public string? SmsApiKey { get; set; }
    public string? SmsSenderId { get; set; }

    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}