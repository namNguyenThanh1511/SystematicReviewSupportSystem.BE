using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class SynthesisExecution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "data_grouping_plan",
                table: "data_synthesis_strategy",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "sensitivity_analysis_plan",
                table: "data_synthesis_strategy",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<List<Guid>>(
                name: "target_research_question_ids",
                table: "data_synthesis_strategy",
                type: "uuid[]",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "synthesis_processes",
                columns: table => new
                {
                    synthesis_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_synthesis_processes", x => x.synthesis_process_id);
                    table.ForeignKey(
                        name: "FK_synthesis_processes_review_processes_review_process_id",
                        column: x => x.review_process_id,
                        principalTable: "review_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "research_question_findings",
                columns: table => new
                {
                    finding_id = table.Column<Guid>(type: "uuid", nullable: false),
                    synthesis_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    research_question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    answer_text = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    author_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_research_question_findings", x => x.finding_id);
                    table.ForeignKey(
                        name: "FK_research_question_findings_research_question_research_quest~",
                        column: x => x.research_question_id,
                        principalTable: "research_question",
                        principalColumn: "research_question_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_research_question_findings_synthesis_processes_synthesis_pr~",
                        column: x => x.synthesis_process_id,
                        principalTable: "synthesis_processes",
                        principalColumn: "synthesis_process_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_research_question_findings_users_author_id",
                        column: x => x.author_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "synthesis_themes",
                columns: table => new
                {
                    synthesis_theme_id = table.Column<Guid>(type: "uuid", nullable: false),
                    synthesis_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    color_code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_synthesis_themes", x => x.synthesis_theme_id);
                    table.ForeignKey(
                        name: "FK_synthesis_themes_synthesis_processes_synthesis_process_id",
                        column: x => x.synthesis_process_id,
                        principalTable: "synthesis_processes",
                        principalColumn: "synthesis_process_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_synthesis_themes_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "theme_evidences",
                columns: table => new
                {
                    theme_evidence_id = table.Column<Guid>(type: "uuid", nullable: false),
                    theme_id = table.Column<Guid>(type: "uuid", nullable: false),
                    extracted_data_value_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_theme_evidences", x => x.theme_evidence_id);
                    table.ForeignKey(
                        name: "FK_theme_evidences_extracted_data_value_extracted_data_value_id",
                        column: x => x.extracted_data_value_id,
                        principalTable: "extracted_data_value",
                        principalColumn: "value_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_theme_evidences_synthesis_themes_theme_id",
                        column: x => x.theme_id,
                        principalTable: "synthesis_themes",
                        principalColumn: "synthesis_theme_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_theme_evidences_users_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_research_question_findings_author_id",
                table: "research_question_findings",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_research_question_findings_research_question_id",
                table: "research_question_findings",
                column: "research_question_id");

            migrationBuilder.CreateIndex(
                name: "IX_research_question_findings_synthesis_process_id",
                table: "research_question_findings",
                column: "synthesis_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_synthesis_processes_review_process_id",
                table: "synthesis_processes",
                column: "review_process_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_synthesis_themes_created_by_id",
                table: "synthesis_themes",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_synthesis_themes_synthesis_process_id",
                table: "synthesis_themes",
                column: "synthesis_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_theme_evidences_created_by_id",
                table: "theme_evidences",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_theme_evidences_extracted_data_value_id",
                table: "theme_evidences",
                column: "extracted_data_value_id");

            migrationBuilder.CreateIndex(
                name: "IX_theme_evidences_theme_id",
                table: "theme_evidences",
                column: "theme_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "research_question_findings");

            migrationBuilder.DropTable(
                name: "theme_evidences");

            migrationBuilder.DropTable(
                name: "synthesis_themes");

            migrationBuilder.DropTable(
                name: "synthesis_processes");

            migrationBuilder.DropColumn(
                name: "data_grouping_plan",
                table: "data_synthesis_strategy");

            migrationBuilder.DropColumn(
                name: "sensitivity_analysis_plan",
                table: "data_synthesis_strategy");

            migrationBuilder.DropColumn(
                name: "target_research_question_ids",
                table: "data_synthesis_strategy");
        }
    }
}
