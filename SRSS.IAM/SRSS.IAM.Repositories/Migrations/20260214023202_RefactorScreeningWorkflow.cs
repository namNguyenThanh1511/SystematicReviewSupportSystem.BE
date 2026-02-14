using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RefactorScreeningWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_prisma_reports_systematic_review_projects_project_id",
                table: "prisma_reports");

            migrationBuilder.DropColumn(
                name: "is_included_final",
                table: "papers");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "prisma_reports",
                newName: "review_process_id");

            migrationBuilder.RenameIndex(
                name: "IX_prisma_reports_project_id",
                table: "prisma_reports",
                newName: "IX_prisma_reports_review_process_id");

            migrationBuilder.CreateTable(
                name: "study_selection_processes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_selection_processes", x => x.id);
                    table.ForeignKey(
                        name: "FK_study_selection_processes_review_processes_review_process_id",
                        column: x => x.review_process_id,
                        principalTable: "review_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "screening_decisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    study_selection_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    decision = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_screening_decisions", x => x.id);
                    table.ForeignKey(
                        name: "FK_screening_decisions_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_screening_decisions_study_selection_processes_study_selecti~",
                        column: x => x.study_selection_process_id,
                        principalTable: "study_selection_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "screening_resolutions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    study_selection_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    final_decision = table.Column<string>(type: "text", nullable: false),
                    resolution_notes = table.Column<string>(type: "text", nullable: true),
                    resolved_by = table.Column<Guid>(type: "uuid", nullable: false),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_screening_resolutions", x => x.id);
                    table.ForeignKey(
                        name: "FK_screening_resolutions_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_screening_resolutions_study_selection_processes_study_selec~",
                        column: x => x.study_selection_process_id,
                        principalTable: "study_selection_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_screening_decisions_paper_id",
                table: "screening_decisions",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_screening_decisions_reviewer_id",
                table: "screening_decisions",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "IX_screening_decisions_study_selection_process_id",
                table: "screening_decisions",
                column: "study_selection_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_screening_decisions_study_selection_process_id_paper_id_rev~",
                table: "screening_decisions",
                columns: new[] { "study_selection_process_id", "paper_id", "reviewer_id" });

            migrationBuilder.CreateIndex(
                name: "IX_screening_resolutions_paper_id",
                table: "screening_resolutions",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_screening_resolutions_resolved_by",
                table: "screening_resolutions",
                column: "resolved_by");

            migrationBuilder.CreateIndex(
                name: "IX_screening_resolutions_study_selection_process_id_paper_id",
                table: "screening_resolutions",
                columns: new[] { "study_selection_process_id", "paper_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_processes_review_process_id",
                table: "study_selection_processes",
                column: "review_process_id");

            migrationBuilder.AddForeignKey(
                name: "FK_prisma_reports_review_processes_review_process_id",
                table: "prisma_reports",
                column: "review_process_id",
                principalTable: "review_processes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_prisma_reports_review_processes_review_process_id",
                table: "prisma_reports");

            migrationBuilder.DropTable(
                name: "screening_decisions");

            migrationBuilder.DropTable(
                name: "screening_resolutions");

            migrationBuilder.DropTable(
                name: "study_selection_processes");

            migrationBuilder.RenameColumn(
                name: "review_process_id",
                table: "prisma_reports",
                newName: "project_id");

            migrationBuilder.RenameIndex(
                name: "IX_prisma_reports_review_process_id",
                table: "prisma_reports",
                newName: "IX_prisma_reports_project_id");

            migrationBuilder.AddColumn<bool>(
                name: "is_included_final",
                table: "papers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_prisma_reports_systematic_review_projects_project_id",
                table: "prisma_reports",
                column: "project_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
