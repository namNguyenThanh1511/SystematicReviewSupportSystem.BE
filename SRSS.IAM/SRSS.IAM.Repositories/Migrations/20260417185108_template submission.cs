using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class templatesubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "version",
                table: "study_selection_checklist_submissions");

            migrationBuilder.CreateTable(
                name: "study_selection_checklist_submission_item_answers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    submission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_checked = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_selection_checklist_submission_item_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_study_selection_checklist_submission_item_answers_study_sel~",
                        column: x => x.item_id,
                        principalTable: "study_selection_checklist_template_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_study_selection_checklist_submission_item_answers_study_se~1",
                        column: x => x.submission_id,
                        principalTable: "study_selection_checklist_submissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "study_selection_checklist_submission_section_answers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    submission_id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_checked = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_selection_checklist_submission_section_answers", x => x.id);
                    table.ForeignKey(
                        name: "FK_study_selection_checklist_submission_section_answers_study_~",
                        column: x => x.section_id,
                        principalTable: "study_selection_checklist_template_sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_study_selection_checklist_submission_section_answers_study~1",
                        column: x => x.submission_id,
                        principalTable: "study_selection_checklist_submissions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_checklist_submission_item_answers_item_id",
                table: "study_selection_checklist_submission_item_answers",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_checklist_submission_item_answers_submissio~",
                table: "study_selection_checklist_submission_item_answers",
                columns: new[] { "submission_id", "item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_checklist_submission_section_answers_sectio~",
                table: "study_selection_checklist_submission_section_answers",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_checklist_submission_section_answers_submis~",
                table: "study_selection_checklist_submission_section_answers",
                columns: new[] { "submission_id", "section_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "study_selection_checklist_submission_item_answers");

            migrationBuilder.DropTable(
                name: "study_selection_checklist_submission_section_answers");

            migrationBuilder.AddColumn<int>(
                name: "version",
                table: "study_selection_checklist_submissions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
