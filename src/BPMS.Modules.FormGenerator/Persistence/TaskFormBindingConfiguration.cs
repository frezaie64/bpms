using BPMS.Modules.FormGenerator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BPMS.Modules.FormGenerator.Persistence;

public class TaskFormBindingConfiguration : IEntityTypeConfiguration<TaskFormBinding>
{
    public void Configure(EntityTypeBuilder<TaskFormBinding> builder)
    {
        builder.ToTable("form_generator_task_bindings");
        builder.HasKey(t => t.TaskId);

        builder.Property(t => t.TaskId)
            .HasColumnName("task_id")
            .HasMaxLength(200);

        builder.Property(t => t.FormId)
            .HasColumnName("form_id");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at");
    }
}