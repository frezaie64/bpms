using BPMS.Modules.Notifications.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BPMS.Modules.Notifications.Persistence;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.Event)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.UserId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.UserName)
            .HasMaxLength(200);

        builder.Property(a => a.Entity)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.EntityId)
            .HasMaxLength(100);

        builder.Property(a => a.Details)
            .HasMaxLength(4000);

        builder.HasIndex(a => new { a.TenantId, a.Event });
        builder.HasIndex(a => new { a.TenantId, a.UserId });
        builder.HasIndex(a => new { a.TenantId, a.Entity });
        builder.HasIndex(a => new { a.TenantId, a.Timestamp });
    }
}