using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddPrisma2020Checklist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "checklist_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
                    version = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checklist_templates", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "checklist_item_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    section = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    topic = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    has_location_field = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    default_sample_answer = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checklist_item_templates", x => x.id);
                    table.ForeignKey(
                        name: "FK_checklist_item_templates_checklist_item_templates_parent_id",
                        column: x => x.parent_id,
                        principalTable: "checklist_item_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_checklist_item_templates_checklist_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "checklist_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "review_checklists",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    completion_percentage = table.Column<double>(type: "double precision", nullable: false),
                    last_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_checklists", x => x.id);
                    table.ForeignKey(
                        name: "FK_review_checklists_checklist_templates_template_id",
                        column: x => x.template_id,
                        principalTable: "checklist_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_review_checklists_systematic_review_projects_review_id",
                        column: x => x.review_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "checklist_item_responses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_checklist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    location = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_not_applicable = table.Column<bool>(type: "boolean", nullable: false),
                    is_reported = table.Column<bool>(type: "boolean", nullable: false),
                    last_updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checklist_item_responses", x => x.id);
                    table.ForeignKey(
                        name: "FK_checklist_item_responses_checklist_item_templates_item_temp~",
                        column: x => x.item_template_id,
                        principalTable: "checklist_item_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_checklist_item_responses_review_checklists_review_checklist~",
                        column: x => x.review_checklist_id,
                        principalTable: "review_checklists",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_checklist_item_responses_item_template_id",
                table: "checklist_item_responses",
                column: "item_template_id");

            migrationBuilder.CreateIndex(
                name: "ux_checklist_item_responses_review_item",
                table: "checklist_item_responses",
                columns: new[] { "review_checklist_id", "item_template_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_checklist_item_templates_parent_id",
                table: "checklist_item_templates",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "idx_checklist_item_templates_template_order",
                table: "checklist_item_templates",
                columns: new[] { "template_id", "order_index" });

            migrationBuilder.CreateIndex(
                name: "ux_checklist_item_templates_template_item_number",
                table: "checklist_item_templates",
                columns: new[] { "template_id", "item_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_checklist_templates_system_name",
                table: "checklist_templates",
                columns: new[] { "is_system", "name" });

            migrationBuilder.CreateIndex(
                name: "idx_review_checklists_review_id",
                table: "review_checklists",
                column: "review_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_checklists_template_id",
                table: "review_checklists",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "ux_review_checklists_review_template",
                table: "review_checklists",
                columns: new[] { "review_id", "template_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "checklist_item_responses");

            migrationBuilder.DropTable(
                name: "checklist_item_templates");

            migrationBuilder.DropTable(
                name: "review_checklists");

            migrationBuilder.DropTable(
                name: "checklist_templates");
        }
    }
}
