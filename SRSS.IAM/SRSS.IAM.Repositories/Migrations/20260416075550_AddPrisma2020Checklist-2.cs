using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddPrisma2020Checklist2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_section_header_only",
                table: "checklist_item_templates",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "section_id",
                table: "checklist_item_templates",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "checklist_section_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    section_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checklist_section_templates", x => x.id);
                    table.ForeignKey(
                        name: "FK_checklist_section_templates_checklist_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "checklist_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_checklist_item_templates_section_id",
                table: "checklist_item_templates",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "idx_checklist_section_templates_template_order",
                table: "checklist_section_templates",
                columns: new[] { "template_id", "order_index" });

            migrationBuilder.CreateIndex(
                name: "ux_checklist_section_templates_template_section_number",
                table: "checklist_section_templates",
                columns: new[] { "template_id", "section_number" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_checklist_item_templates_checklist_section_templates_sectio~",
                table: "checklist_item_templates",
                column: "section_id",
                principalTable: "checklist_section_templates",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_checklist_item_templates_checklist_section_templates_sectio~",
                table: "checklist_item_templates");

            migrationBuilder.DropTable(
                name: "checklist_section_templates");

            migrationBuilder.DropIndex(
                name: "idx_checklist_item_templates_section_id",
                table: "checklist_item_templates");

            migrationBuilder.DropColumn(
                name: "is_section_header_only",
                table: "checklist_item_templates");

            migrationBuilder.DropColumn(
                name: "section_id",
                table: "checklist_item_templates");
        }
    }
}
