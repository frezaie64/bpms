using BPMS.Modules.FormGenerator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BPMS.Modules.FormGenerator.Persistence;

public class FormFieldConfiguration : IEntityTypeConfiguration<FormField>
{
    public void Configure(EntityTypeBuilder<FormField> builder)
    {
        builder.ToTable("form_generator_fields");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .HasColumnName("id");

        builder.Property(f => f.FormId)
            .HasColumnName("form_id");

        builder.Property(f => f.Key)
            .HasColumnName("field_key")
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(f => f.Label)
            .HasColumnName("label")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.Type)
            .HasColumnName("field_type")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(f => f.Required)
            .HasColumnName("required");

        builder.Property(f => f.Placeholder)
            .HasColumnName("placeholder")
            .HasMaxLength(500);

        builder.Property(f => f.Options)
            .HasColumnName("options")
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        builder.Property(f => f.OrderIndex)
            .HasColumnName("order_index");

        builder.HasIndex(f => new { f.FormId, f.OrderIndex })
            .HasDatabaseName("ix_gen_fields_form_id_order_index");

        builder.HasAlternateKey(f => new { f.FormId, f.Key })
            .HasName("ak_gen_fields_form_id_field_key");
    }
}