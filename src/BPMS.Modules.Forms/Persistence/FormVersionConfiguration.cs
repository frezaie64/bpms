using BPMS.Modules.Forms.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BPMS.Modules.Forms.Persistence;

public class FormVersionConfiguration : IEntityTypeConfiguration<FormVersion>
{
    public void Configure(EntityTypeBuilder<FormVersion> builder)
    {
        builder.ToTable("FormVersions");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.JsonDefinition)
            .IsRequired();

        builder.Property(v => v.Notes)
            .HasMaxLength(500);

        builder.HasOne(v => v.Form)
            .WithMany(f => f.Versions)
            .HasForeignKey(v => v.FormId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => new { v.FormId, v.VersionNumber }).IsUnique();
    }
}