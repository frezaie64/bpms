using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BPMS.Shared.Migrations
{
    /// <inheritdoc />
    public partial class PlanAiFormGenerationQuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxStorageMb",
                table: "Plans");

            migrationBuilder.AddColumn<int>(
                name: "MaxAiFormGenerations",
                table: "Plans",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.CreateTable(
                name: "form_generator_forms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    tenant_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    prompt = table.Column<string>(type: "text", nullable: false),
                    creator_user_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: ""),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expire_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_generator_forms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "form_generator_fields",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    form_id = table.Column<Guid>(type: "uuid", nullable: false),
                    field_key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    field_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    required = table.Column<bool>(type: "boolean", nullable: false),
                    placeholder = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    options = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    order_index = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_generator_fields", x => x.id);
                    table.UniqueConstraint("ak_gen_fields_form_id_field_key", x => new { x.form_id, x.field_key });
                    table.ForeignKey(
                        name: "FK_form_generator_fields_form_generator_forms_form_id",
                        column: x => x.form_id,
                        principalTable: "form_generator_forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form_generator_submissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    form_id = table.Column<Guid>(type: "uuid", nullable: false),
                    submitted_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    data = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: false),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_generator_submissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_form_generator_submissions_form_generator_forms_form_id",
                        column: x => x.form_id,
                        principalTable: "form_generator_forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "form_generator_task_bindings",
                columns: table => new
                {
                    task_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    form_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_form_generator_task_bindings", x => x.task_id);
                    table.ForeignKey(
                        name: "FK_form_generator_task_bindings_form_generator_forms_form_id",
                        column: x => x.form_id,
                        principalTable: "form_generator_forms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_gen_fields_form_id_order_index",
                table: "form_generator_fields",
                columns: new[] { "form_id", "order_index" });

            migrationBuilder.CreateIndex(
                name: "IX_form_generator_forms_tenant_id_name",
                table: "form_generator_forms",
                columns: new[] { "tenant_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_gen_forms_creator_user_id",
                table: "form_generator_forms",
                column: "creator_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_gen_submissions_form_id_submitted_at",
                table: "form_generator_submissions",
                columns: new[] { "form_id", "submitted_at" });

            migrationBuilder.CreateIndex(
                name: "IX_form_generator_task_bindings_form_id",
                table: "form_generator_task_bindings",
                column: "form_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "form_generator_fields");

            migrationBuilder.DropTable(
                name: "form_generator_submissions");

            migrationBuilder.DropTable(
                name: "form_generator_task_bindings");

            migrationBuilder.DropTable(
                name: "form_generator_forms");

            migrationBuilder.DropColumn(
                name: "MaxAiFormGenerations",
                table: "Plans");

            migrationBuilder.AddColumn<int>(
                name: "MaxStorageMb",
                table: "Plans",
                type: "integer",
                nullable: false,
                defaultValue: 100);
        }
    }
}
