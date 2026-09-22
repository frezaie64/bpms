using BPMS.Modules.WorkflowDefinitions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BPMS.Modules.WorkflowDefinitions.Persistence;

public class WorkflowCategoryConfiguration : IEntityTypeConfiguration<WorkflowCategory>
{
    public void Configure(EntityTypeBuilder<WorkflowCategory> builder)
    {
        builder.ToTable("WorkflowCategories");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.TenantId).IsRequired().HasMaxLength(100);
        builder.Property(c => c.CreatedBy).IsRequired().HasMaxLength(100);
        builder.Property(c => c.UpdatedBy).HasMaxLength(100);
        builder.HasIndex(c => new { c.TenantId, c.Name }).IsUnique();
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}