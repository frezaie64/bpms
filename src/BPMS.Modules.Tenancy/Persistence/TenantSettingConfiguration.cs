using BPMS.Modules.Tenancy.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BPMS.Modules.Tenancy.Persistence;

public class TenantSettingConfiguration : IEntityTypeConfiguration<TenantSetting>
{
    public void Configure(EntityTypeBuilder<TenantSetting> builder)
    {
        builder.ToTable("TenantSettings");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.TenantId).IsRequired().HasMaxLength(100);
        builder.Property(s => s.TenantName).HasMaxLength(200);
        builder.Property(s => s.Logo).HasMaxLength(500);
        builder.Property(s => s.TimeZone).HasMaxLength(100);
        builder.Property(s => s.DateFormat).HasMaxLength(50);
        builder.Property(s => s.Language).HasMaxLength(50);
        builder.Property(s => s.SmtpHost).HasMaxLength(200);
        builder.Property(s => s.SmtpUsername).HasMaxLength(200);
        builder.Property(s => s.SmtpPassword).HasMaxLength(500);
        builder.Property(s => s.SenderName).HasMaxLength(200);
        builder.Property(s => s.SenderEmail).HasMaxLength(200);
        builder.Property(s => s.SmsProviderName).HasMaxLength(200);
        builder.Property(s => s.SmsApiUrl).HasMaxLength(500);
        builder.Property(s => s.SmsApiKey).HasMaxLength(500);
        builder.Property(s => s.SmsSenderId).HasMaxLength(100);
        builder.Property(s => s.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(s => s.UpdatedBy).HasMaxLength(100);
        builder.HasIndex(s => s.TenantId).IsUnique();
    }
}