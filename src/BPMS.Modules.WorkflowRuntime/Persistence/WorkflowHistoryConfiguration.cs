using BPMS.Modules.WorkflowRuntime.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BPMS.Modules.WorkflowRuntime.Persistence;

public class WorkflowHistoryConfiguration : IEntityTypeConfiguration<WorkflowHistory>
{
    public void Configure(EntityTypeBuilder<WorkflowHistory> builder)
    {
        builder.ToTable("WorkflowHistory");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EventType).HasMaxLength(50).IsRequired();
        builder.Property(x => x.NodeId).HasMaxLength(100);
        builder.Property(x => x.NodeName).HasMaxLength(200);
        builder.Property(x => x.Data).HasColumnType("text");
        builder.Property(x => x.PerformedBy).HasMaxLength(100);

        builder.HasIndex(x => new { x.WorkflowInstanceId, x.Timestamp });
    }
}