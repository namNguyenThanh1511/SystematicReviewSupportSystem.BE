using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "project_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewers_per_paper_screening = table.Column<int>(type: "integer", nullable: false),
                    reviewers_per_paper_quality = table.Column<int>(type: "integer", nullable: false),
                    reviewers_per_paper_extraction = table.Column<int>(type: "integer", nullable: false),
                    auto_resolve_screening_conflicts = table.Column<bool>(type: "boolean", nullable: false),
                    auto_resolve_quality_conflicts = table.Column<bool>(type: "boolean", nullable: false),
                    auto_resolve_extraction_conflicts = table.Column<bool>(type: "boolean", nullable: false),
                    deduplication_strictness = table.Column<string>(type: "text", nullable: false),
                    auto_resolve_duplicates = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_settings_systematic_review_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_project_settings_project_id",
                table: "project_settings",
                column: "project_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "project_settings");
        }
    }
}
