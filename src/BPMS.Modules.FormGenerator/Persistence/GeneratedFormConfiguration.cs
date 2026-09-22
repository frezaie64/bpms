using BPMS.Modules.FormGenerator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BPMS.Modules.FormGenerator.Persistence;

public class GeneratedFormConfiguration : IEntityTypeConfiguration<GeneratedForm>
{
    public void Configure(EntityTypeBuilder<GeneratedForm> builder)
    {
        builder.ToTable("form_generator_forms");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .HasColumnName("name")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(f => f.Prompt)
            .HasColumnName("prompt");

        builder.Property(f => f.CreatorUserId)
            .HasColumnName("creator_user_id")
            .HasMaxLength(100)
            .HasDefaultValue("");

        builder.Property(f => f.StartDate)
            .HasColumnName("start_date");

        builder.Property(f => f.ExpireDate)
            .HasColumnName("expire_date");

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(f => f.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(f => f.UpdatedBy)
            .HasColumnName("updated_by")
            .HasMaxLength(100);

        builder.Property(f => f.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired()
            .HasMaxLength(100);

        builder.HasMany(f => f.Fields)
            .WithOne(f => f.Form)
            .HasForeignKey(f => f.FormId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(f => f.Submissions)
            .WithOne(s => s.Form)
            .HasForeignKey(s => s.FormId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => f.CreatorUserId)
            .HasDatabaseName("ix_gen_forms_creator_user_id");

        builder.HasIndex(f => new { f.TenantId, f.Name });
    }
}