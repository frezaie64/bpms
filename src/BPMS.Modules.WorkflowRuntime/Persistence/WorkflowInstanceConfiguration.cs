using BPMS.Modules.WorkflowRuntime.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BPMS.Modules.WorkflowRuntime.Persistence;

public class WorkflowInstanceConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public void Configure(EntityTypeBuilder<WorkflowInstance> builder)
    {
        builder.ToTable("WorkflowInstances");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.WorkflowName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.StartedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ActiveNodeIds).HasColumnType("text");
        builder.Property(x => x.CompletedNodeIds).HasColumnType("text");
        builder.Property(x => x.Variables).HasColumnType("text");
        builder.Property(x => x.JsonDefinition).HasColumnType("text");

        builder.HasIndex(x => new { x.TenantId, x.WorkflowId });
        builder.HasIndex(x => x.Status);

        builder.HasMany(x => x.Tasks)
            .WithOne(t => t.WorkflowInstance)
            .HasForeignKey(t => t.WorkflowInstanceId);

        builder.HasMany(x => x.History)
            .WithOne(h => h.WorkflowInstance)
            .HasForeignKey(h => h.WorkflowInstanceId);
    }
}