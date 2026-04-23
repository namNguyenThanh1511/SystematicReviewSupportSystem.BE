using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class multiple_criteria_process_relationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_study_selection_criteria_study_selection_process_id",
                table: "study_selection_criteria");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_criteria_study_selection_process_id",
                table: "study_selection_criteria",
                column: "study_selection_process_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_study_selection_criteria_study_selection_process_id",
                table: "study_selection_criteria");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_criteria_study_selection_process_id",
                table: "study_selection_criteria",
                column: "study_selection_process_id",
                unique: true);
        }
    }
}
