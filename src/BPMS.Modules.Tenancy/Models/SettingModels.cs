namespace BPMS.Modules.Tenancy.Models;

public record TenantSettingResponse(
    Guid Id,
    string? TenantName,
    string? Logo,
    string? TimeZone,
    string? DateFormat,
    string? Language,
    int? DefaultFileSizeLimit,
    int? DefaultPageSize,
    string? SmtpHost,
    int? SmtpPort,
    string? SmtpUsername,
    string? SenderName,
    string? SenderEmail,
    string? SmsProviderName,
    string? SmsApiUrl,
    string? SmsSenderId,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record UpdateTenantSettingRequest(
    string? TenantName,
    string? Logo,
    string? TimeZone,
    string? DateFormat,
    string? Language,
    int? DefaultFileSizeLimit,
    int? DefaultPageSize,
    string? SmtpHost,
    int? SmtpPort,
    string? SmtpUsername,
    string? SmtpPassword,
    string? SenderName,
    string? SenderEmail,
    string? SmsProviderName,
    string? SmsApiUrl,
    string? SmsApiKey,
    string? SmsSenderId
);