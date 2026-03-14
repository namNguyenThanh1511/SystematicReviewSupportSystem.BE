using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class StudySelection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "screening_phase",
                table: "screening_resolutions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "exclusion_reason_code",
                table: "screening_decisions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reviewer_notes",
                table: "screening_decisions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "screening_phase",
                table: "screening_decisions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "full_text_screenings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    study_selection_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    min_reviewers_per_paper = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    max_reviewers_per_paper = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_full_text_screenings", x => x.id);
                    table.ForeignKey(
                        name: "FK_full_text_screenings_study_selection_processes_study_select~",
                        column: x => x.study_selection_process_id,
                        principalTable: "study_selection_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "title_abstract_screenings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    study_selection_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    min_reviewers_per_paper = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    max_reviewers_per_paper = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_title_abstract_screenings", x => x.id);
                    table.ForeignKey(
                        name: "FK_title_abstract_screenings_study_selection_processes_study_s~",
                        column: x => x.study_selection_process_id,
                        principalTable: "study_selection_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_ft_screening_study_selection_process_id_unique",
                table: "full_text_screenings",
                column: "study_selection_process_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_ta_screening_study_selection_process_id_unique",
                table: "title_abstract_screenings",
                column: "study_selection_process_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "full_text_screenings");

            migrationBuilder.DropTable(
                name: "title_abstract_screenings");

            migrationBuilder.DropColumn(
                name: "screening_phase",
                table: "screening_resolutions");

            migrationBuilder.DropColumn(
                name: "exclusion_reason_code",
                table: "screening_decisions");

            migrationBuilder.DropColumn(
                name: "reviewer_notes",
                table: "screening_decisions");

            migrationBuilder.DropColumn(
                name: "screening_phase",
                table: "screening_decisions");
        }
    }
}
