using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BPMS.Modules.Tenancy.Entities;

namespace BPMS.Modules.Tenancy.Persistence;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        builder.Property(p => p.Status).HasConversion<int>();
        builder.Property(p => p.PaymentMethod).HasMaxLength(50);
        builder.Property(p => p.TransactionId).HasMaxLength(200);
        builder.Property(p => p.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(p => p.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(p => p.SubscriptionId);
        builder.HasIndex(p => new { p.UserId, p.Status });
    }
}
