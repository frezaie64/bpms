using BPMS.Modules.FileManagement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BPMS.Modules.FileManagement.Persistence;

public class FileConfiguration : IEntityTypeConfiguration<FileItem>
{
    public void Configure(EntityTypeBuilder<FileItem> builder)
    {
        builder.ToTable("Files");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(f => f.OriginalFileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(f => f.ContentType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.ObjectName)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(f => f.BucketName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Category)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.UpdatedBy)
            .HasMaxLength(100);

        builder.HasIndex(f => new { f.TenantId, f.Category });
        builder.HasIndex(f => new { f.TenantId, f.OriginalFileName });
        builder.HasQueryFilter(f => !f.IsDeleted);
    }
}