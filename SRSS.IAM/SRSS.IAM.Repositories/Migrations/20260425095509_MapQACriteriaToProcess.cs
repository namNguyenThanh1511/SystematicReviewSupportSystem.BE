using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class MapQACriteriaToProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_quality_assessment_strategy_systematic_review_projects_proj~",
                table: "quality_assessment_strategy");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "quality_assessment_strategy",
                newName: "review_process_id");

            migrationBuilder.RenameIndex(
                name: "IX_quality_assessment_strategy_project_id",
                table: "quality_assessment_strategy",
                newName: "IX_quality_assessment_strategy_review_process_id");

            migrationBuilder.AddForeignKey(
                name: "FK_quality_assessment_strategy_review_processes_review_process~",
                table: "quality_assessment_strategy",
                column: "review_process_id",
                principalTable: "review_processes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_quality_assessment_strategy_review_processes_review_process~",
                table: "quality_assessment_strategy");

            migrationBuilder.RenameColumn(
                name: "review_process_id",
                table: "quality_assessment_strategy",
                newName: "project_id");

            migrationBuilder.RenameIndex(
                name: "IX_quality_assessment_strategy_review_process_id",
                table: "quality_assessment_strategy",
                newName: "IX_quality_assessment_strategy_project_id");

            migrationBuilder.AddForeignKey(
                name: "FK_quality_assessment_strategy_systematic_review_projects_proj~",
                table: "quality_assessment_strategy",
                column: "project_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
