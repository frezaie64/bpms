using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BPMS.Modules.Tenancy.Entities;

namespace BPMS.Modules.Tenancy.Persistence;

public class TenantMemberConfiguration : IEntityTypeConfiguration<TenantMember>
{
    public void Configure(EntityTypeBuilder<TenantMember> builder)
    {
        builder.ToTable("TenantMembers");
        builder.HasKey(tm => new { tm.TenantId, tm.UserId });
        builder.Property(tm => tm.Role).HasConversion<int>();

        builder.HasOne(tm => tm.Tenant)
            .WithMany(t => t.Members)
            .HasForeignKey(tm => tm.TenantId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}