using BPMS.Modules.Forms.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BPMS.Modules.Forms.Persistence;

public class FormConfiguration : IEntityTypeConfiguration<Form>
{
    public void Configure(EntityTypeBuilder<Form> builder)
    {
        builder.ToTable("Forms");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Description)
            .HasMaxLength(1000);

        builder.Property(f => f.JsonDefinition)
            .IsRequired();

        builder.Property(f => f.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.UpdatedBy)
            .HasMaxLength(100);

        builder.HasOne(f => f.Category)
            .WithMany(c => c.Forms)
            .HasForeignKey(f => f.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(f => new { f.TenantId, f.Name });
        builder.HasQueryFilter(f => !f.IsDeleted);
    }
}