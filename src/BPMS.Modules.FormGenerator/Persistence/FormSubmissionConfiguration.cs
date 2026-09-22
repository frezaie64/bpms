using BPMS.Modules.FormGenerator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BPMS.Modules.FormGenerator.Persistence;

public class FormSubmissionConfiguration : IEntityTypeConfiguration<FormSubmission>
{
    public void Configure(EntityTypeBuilder<FormSubmission> builder)
    {
        builder.ToTable("form_generator_submissions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.FormId)
            .HasColumnName("form_id");

        builder.Property(s => s.SubmittedBy)
            .HasColumnName("submitted_by")
            .HasMaxLength(100);

        builder.Property(s => s.Data)
            .HasColumnName("data")
            .HasColumnType("jsonb");

        builder.Property(s => s.SubmittedAt)
            .HasColumnName("submitted_at");

        builder.HasIndex(s => new { s.FormId, s.SubmittedAt })
            .HasDatabaseName("ix_gen_submissions_form_id_submitted_at");
    }
}