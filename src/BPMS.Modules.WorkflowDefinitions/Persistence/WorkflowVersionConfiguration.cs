using BPMS.Modules.WorkflowDefinitions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BPMS.Modules.WorkflowDefinitions.Persistence;

public class WorkflowVersionConfiguration : IEntityTypeConfiguration<WorkflowVersion>
{
    public void Configure(EntityTypeBuilder<WorkflowVersion> builder)
    {
        builder.ToTable("WorkflowVersions");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.JsonDefinition)
            .IsRequired();

        builder.Property(v => v.Notes)
            .HasMaxLength(500);

        builder.HasOne(v => v.Workflow)
            .WithMany(w => w.Versions)
            .HasForeignKey(v => v.WorkflowId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => new { v.WorkflowId, v.VersionNumber }).IsUnique();
    }
}