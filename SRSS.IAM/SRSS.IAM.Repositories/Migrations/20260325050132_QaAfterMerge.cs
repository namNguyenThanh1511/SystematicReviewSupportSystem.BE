using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class QaAfterMerge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "quality_assessment_processes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quality_assessment_processes", x => x.id);
                    table.ForeignKey(
                        name: "FK_quality_assessment_processes_review_processes_review_proces~",
                        column: x => x.review_process_id,
                        principalTable: "review_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quality_assessment_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quality_assessment_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quality_assessment_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_quality_assessment_assignments_quality_assessment_processes~",
                        column: x => x.quality_assessment_process_id,
                        principalTable: "quality_assessment_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quality_assessment_assignments_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quality_assessment_decisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quality_assessment_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    score = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quality_assessment_decisions", x => x.id);
                    table.ForeignKey(
                        name: "FK_quality_assessment_decisions_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quality_assessment_decisions_quality_assessment_processes_q~",
                        column: x => x.quality_assessment_process_id,
                        principalTable: "quality_assessment_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quality_assessment_decisions_users_reviewer_id",
                        column: x => x.reviewer_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quality_assessment_resolutions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quality_assessment_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    resolved_by = table.Column<Guid>(type: "uuid", nullable: false),
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    final_decision = table.Column<int>(type: "integer", nullable: false),
                    final_score = table.Column<decimal>(type: "numeric", nullable: true),
                    resolution_notes = table.Column<string>(type: "text", nullable: true),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quality_assessment_resolutions", x => x.id);
                    table.ForeignKey(
                        name: "FK_quality_assessment_resolutions_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quality_assessment_resolutions_quality_assessment_processes~",
                        column: x => x.quality_assessment_process_id,
                        principalTable: "quality_assessment_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quality_assessment_resolutions_users_resolved_by",
                        column: x => x.resolved_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "quality_assessment_assignment_papers",
                columns: table => new
                {
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quality_assessment_assignment_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quality_assessment_assignment_papers", x => new { x.paper_id, x.quality_assessment_assignment_id });
                    table.ForeignKey(
                        name: "FK_quality_assessment_assignment_papers_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quality_assessment_assignment_papers_quality_assessment_ass~",
                        column: x => x.quality_assessment_assignment_id,
                        principalTable: "quality_assessment_assignments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quality_assessment_decision_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quality_assessment_decision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quality_criterion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: true),
                    comment = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quality_assessment_decision_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_quality_assessment_decision_items_quality_assessment_decisi~",
                        column: x => x.quality_assessment_decision_id,
                        principalTable: "quality_assessment_decisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quality_assessment_decision_items_quality_criterion_quality~",
                        column: x => x.quality_criterion_id,
                        principalTable: "quality_criterion",
                        principalColumn: "quality_criterion_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_assignment_papers_quality_assessment_ass~",
                table: "quality_assessment_assignment_papers",
                column: "quality_assessment_assignment_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_assignments_quality_assessment_process_id",
                table: "quality_assessment_assignments",
                column: "quality_assessment_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_assignments_user_id",
                table: "quality_assessment_assignments",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_decision_items_quality_assessment_decisi~",
                table: "quality_assessment_decision_items",
                column: "quality_assessment_decision_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_decision_items_quality_criterion_id",
                table: "quality_assessment_decision_items",
                column: "quality_criterion_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_decisions_paper_id",
                table: "quality_assessment_decisions",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_decisions_quality_assessment_process_id",
                table: "quality_assessment_decisions",
                column: "quality_assessment_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_decisions_reviewer_id",
                table: "quality_assessment_decisions",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_processes_review_process_id",
                table: "quality_assessment_processes",
                column: "review_process_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_resolutions_paper_id",
                table: "quality_assessment_resolutions",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_resolutions_quality_assessment_process_id",
                table: "quality_assessment_resolutions",
                column: "quality_assessment_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_resolutions_resolved_by",
                table: "quality_assessment_resolutions",
                column: "resolved_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quality_assessment_assignment_papers");

            migrationBuilder.DropTable(
                name: "quality_assessment_decision_items");

            migrationBuilder.DropTable(
                name: "quality_assessment_resolutions");

            migrationBuilder.DropTable(
                name: "quality_assessment_assignments");

            migrationBuilder.DropTable(
                name: "quality_assessment_decisions");

            migrationBuilder.DropTable(
                name: "quality_assessment_processes");
        }
    }
}
