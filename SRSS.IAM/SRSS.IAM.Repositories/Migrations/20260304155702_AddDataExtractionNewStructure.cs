using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddDataExtractionNewStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "extraction_templates",
                columns: table => new
                {
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extraction_templates", x => x.template_id);
                    table.ForeignKey(
                        name: "FK_extraction_templates_review_protocol_protocol_id",
                        column: x => x.protocol_id,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "extraction_fields",
                columns: table => new
                {
                    field_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_field_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    instruction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    field_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extraction_fields", x => x.field_id);
                    table.ForeignKey(
                        name: "FK_extraction_fields_extraction_fields_parent_field_id",
                        column: x => x.parent_field_id,
                        principalTable: "extraction_fields",
                        principalColumn: "field_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_extraction_fields_extraction_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "extraction_templates",
                        principalColumn: "template_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "field_options",
                columns: table => new
                {
                    option_id = table.Column<Guid>(type: "uuid", nullable: false),
                    field_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_field_options", x => x.option_id);
                    table.ForeignKey(
                        name: "FK_field_options_extraction_fields_field_id",
                        column: x => x.field_id,
                        principalTable: "extraction_fields",
                        principalColumn: "field_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "extracted_data_values",
                columns: table => new
                {
                    value_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    field_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    option_id = table.Column<Guid>(type: "uuid", nullable: true),
                    string_value = table.Column<string>(type: "text", nullable: true),
                    numeric_value = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extracted_data_values", x => x.value_id);
                    table.ForeignKey(
                        name: "FK_extracted_data_values_extraction_fields_field_id",
                        column: x => x.field_id,
                        principalTable: "extraction_fields",
                        principalColumn: "field_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_extracted_data_values_field_options_option_id",
                        column: x => x.option_id,
                        principalTable: "field_options",
                        principalColumn: "option_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_extracted_data_values_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_extracted_data_values_users_reviewer_id",
                        column: x => x.reviewer_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_extracted_data_values_field_id",
                table: "extracted_data_values",
                column: "field_id");

            migrationBuilder.CreateIndex(
                name: "IX_extracted_data_values_option_id",
                table: "extracted_data_values",
                column: "option_id");

            migrationBuilder.CreateIndex(
                name: "IX_extracted_data_values_paper_id",
                table: "extracted_data_values",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_extracted_data_values_paper_id_field_id",
                table: "extracted_data_values",
                columns: new[] { "paper_id", "field_id" });

            migrationBuilder.CreateIndex(
                name: "IX_extracted_data_values_reviewer_id",
                table: "extracted_data_values",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "IX_extraction_fields_parent_field_id",
                table: "extraction_fields",
                column: "parent_field_id");

            migrationBuilder.CreateIndex(
                name: "IX_extraction_fields_template_id",
                table: "extraction_fields",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_extraction_templates_protocol_id",
                table: "extraction_templates",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_field_options_field_id",
                table: "field_options",
                column: "field_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "extracted_data_values");

            migrationBuilder.DropTable(
                name: "field_options");

            migrationBuilder.DropTable(
                name: "extraction_fields");

            migrationBuilder.DropTable(
                name: "extraction_templates");
        }
    }
}
