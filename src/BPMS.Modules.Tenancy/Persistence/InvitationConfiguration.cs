using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BPMS.Modules.Tenancy.Entities;

namespace BPMS.Modules.Tenancy.Persistence;

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("Invitations");
        builder.HasKey(i => i.Id);
        builder.Property(i => i.Email).IsRequired().HasMaxLength(256);
        builder.Property(i => i.Role).HasConversion<int>();
        builder.Property(i => i.Token).IsRequired().HasMaxLength(500);
        builder.Property(i => i.Status).HasConversion<int>();
        builder.Property(i => i.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(i => i.UpdatedBy).HasMaxLength(100);

        builder.HasIndex(i => i.Token).IsUnique();
        builder.HasIndex(i => new { i.TenantId, i.Email });
    }
}
