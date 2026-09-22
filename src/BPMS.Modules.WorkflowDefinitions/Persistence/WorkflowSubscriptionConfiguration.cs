using BPMS.Modules.WorkflowDefinitions.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BPMS.Modules.WorkflowDefinitions.Persistence;

public class WorkflowSubscriptionConfiguration : IEntityTypeConfiguration<WorkflowSubscription>
{
    public void Configure(EntityTypeBuilder<WorkflowSubscription> builder)
    {
        builder.ToTable("WorkflowSubscriptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TenantId).HasMaxLength(50).IsRequired();
        builder.Property(x => x.UserId).HasMaxLength(100).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.WorkflowId, x.UserId }).IsUnique();
    }
}