using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class stusechecklisttemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "study_selection_checklist_templates",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_selection_checklist_templates", x => x.id);
                    table.ForeignKey(
                        name: "FK_study_selection_checklist_templates_systematic_review_proje~",
                        column: x => x.project_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "study_selection_checklist_submissions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    screening_decision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    checklist_template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    submitted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false),
                    PaperId = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_selection_checklist_submissions", x => x.id);
                    table.ForeignKey(
                        name: "FK_study_selection_checklist_submissions_papers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "papers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_study_selection_checklist_submissions_screening_decisions_s~",
                        column: x => x.screening_decision_id,
                        principalTable: "screening_decisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_study_selection_checklist_submissions_study_selection_check~",
                        column: x => x.checklist_template_id,
                        principalTable: "study_selection_checklist_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "study_selection_checklist_template_sections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    template_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_selection_checklist_template_sections", x => x.id);
                    table.ForeignKey(
                        name: "FK_study_selection_checklist_template_sections_study_selection~",
                        column: x => x.template_id,
                        principalTable: "study_selection_checklist_templates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "study_selection_checklist_template_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_selection_checklist_template_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_study_selection_checklist_template_items_study_selection_ch~",
                        column: x => x.section_id,
                        principalTable: "study_selection_checklist_template_sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_checklist_submissions_checklist_template_id",
                table: "study_selection_checklist_submissions",
                column: "checklist_template_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_checklist_submissions_PaperId",
                table: "study_selection_checklist_submissions",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_checklist_submissions_screening_decision_id",
                table: "study_selection_checklist_submissions",
                column: "screening_decision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_checklist_template_items_section_id",
                table: "study_selection_checklist_template_items",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_checklist_template_sections_template_id",
                table: "study_selection_checklist_template_sections",
                column: "template_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_checklist_templates_project_id",
                table: "study_selection_checklist_templates",
                column: "project_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "study_selection_checklist_submissions");

            migrationBuilder.DropTable(
                name: "study_selection_checklist_template_items");

            migrationBuilder.DropTable(
                name: "study_selection_checklist_template_sections");

            migrationBuilder.DropTable(
                name: "study_selection_checklist_templates");
        }
    }
}
