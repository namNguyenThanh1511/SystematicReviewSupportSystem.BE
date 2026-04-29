using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RefactorQualityAssessmentStrategyToQAProcess : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_quality_assessment_strategy_review_processes_review_process~",
                table: "quality_assessment_strategy");

            migrationBuilder.RenameColumn(
                name: "review_process_id",
                table: "quality_assessment_strategy",
                newName: "quality_assessment_process_id");

            migrationBuilder.RenameIndex(
                name: "IX_quality_assessment_strategy_review_process_id",
                table: "quality_assessment_strategy",
                newName: "IX_quality_assessment_strategy_quality_assessment_process_id");

            migrationBuilder.AddForeignKey(
                name: "FK_quality_assessment_strategy_quality_assessment_processes_qu~",
                table: "quality_assessment_strategy",
                column: "quality_assessment_process_id",
                principalTable: "quality_assessment_processes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_quality_assessment_strategy_quality_assessment_processes_qu~",
                table: "quality_assessment_strategy");

            migrationBuilder.RenameColumn(
                name: "quality_assessment_process_id",
                table: "quality_assessment_strategy",
                newName: "review_process_id");

            migrationBuilder.RenameIndex(
                name: "IX_quality_assessment_strategy_quality_assessment_process_id",
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
    }
}
