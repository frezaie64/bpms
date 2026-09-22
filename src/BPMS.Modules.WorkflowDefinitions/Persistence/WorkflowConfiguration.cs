using BPMS.Modules.WorkflowDefinitions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BPMS.Modules.WorkflowDefinitions.Persistence;

public class WorkflowConfiguration : IEntityTypeConfiguration<Workflow>
{
    public void Configure(EntityTypeBuilder<Workflow> builder)
    {
        builder.ToTable("Workflows");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(w => w.Description)
            .HasMaxLength(1000);

        builder.Property(w => w.JsonDefinition)
            .IsRequired();

        builder.Property(w => w.TenantId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.UpdatedBy)
            .HasMaxLength(100);

        builder.HasIndex(w => new { w.TenantId, w.Name });
        builder.HasQueryFilter(w => !w.IsDeleted);
    }
}