using BPMS.Modules.Tenancy.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BPMS.Modules.Tenancy.Persistence;

public class LookupValueConfiguration : IEntityTypeConfiguration<LookupValue>
{
    public void Configure(EntityTypeBuilder<LookupValue> builder)
    {
        builder.ToTable("LookupValues");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.TenantId).IsRequired().HasMaxLength(100);
        builder.Property(l => l.Group).IsRequired().HasMaxLength(100);
        builder.Property(l => l.Name).IsRequired().HasMaxLength(200);
        builder.Property(l => l.Value).HasMaxLength(500);
        builder.Property(l => l.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(l => l.UpdatedBy).HasMaxLength(100);
        builder.HasIndex(l => new { l.TenantId, l.Group, l.Name }).IsUnique();
        builder.HasQueryFilter(l => !l.IsDeleted);
    }
}