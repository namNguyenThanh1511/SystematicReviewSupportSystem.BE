using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class stusecriteria_relationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_study_selection_criteria_systematic_review_projects_project~",
                table: "study_selection_criteria");

            migrationBuilder.DropIndex(
                name: "IX_study_selection_criteria_project_id",
                table: "study_selection_criteria");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "study_selection_criteria",
                newName: "study_selection_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_criteria_study_selection_process_id",
                table: "study_selection_criteria",
                column: "study_selection_process_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_study_selection_criteria_study_selection_processes_study_se~",
                table: "study_selection_criteria",
                column: "study_selection_process_id",
                principalTable: "study_selection_processes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_study_selection_criteria_study_selection_processes_study_se~",
                table: "study_selection_criteria");

            migrationBuilder.DropIndex(
                name: "IX_study_selection_criteria_study_selection_process_id",
                table: "study_selection_criteria");

            migrationBuilder.RenameColumn(
                name: "study_selection_process_id",
                table: "study_selection_criteria",
                newName: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_criteria_project_id",
                table: "study_selection_criteria",
                column: "project_id");

            migrationBuilder.AddForeignKey(
                name: "FK_study_selection_criteria_systematic_review_projects_project~",
                table: "study_selection_criteria",
                column: "project_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
