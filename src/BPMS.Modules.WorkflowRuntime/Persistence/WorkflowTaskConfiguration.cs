using BPMS.Modules.WorkflowRuntime.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BPMS.Modules.WorkflowRuntime.Persistence;

public class WorkflowTaskConfiguration : IEntityTypeConfiguration<WorkflowTask>
{
    public void Configure(EntityTypeBuilder<WorkflowTask> builder)
    {
        builder.ToTable("WorkflowTasks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.NodeId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NodeName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.AssignedToUserId).HasMaxLength(100);
        builder.Property(x => x.AssignedToRole).HasMaxLength(100);
        builder.Property(x => x.CompletedBy).HasMaxLength(100);
        builder.Property(x => x.Comments).HasMaxLength(2000);
        builder.Property(x => x.FormData).HasColumnType("text");

        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => new { x.TenantId, x.AssignedToUserId, x.Status });
    }
}