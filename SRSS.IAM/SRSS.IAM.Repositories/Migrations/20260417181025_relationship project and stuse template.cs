using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class relationshipprojectandstusetemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_study_selection_checklist_templates_project_id",
                table: "study_selection_checklist_templates");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_checklist_templates_project_id",
                table: "study_selection_checklist_templates",
                column: "project_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_study_selection_checklist_templates_project_id",
                table: "study_selection_checklist_templates");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_checklist_templates_project_id",
                table: "study_selection_checklist_templates",
                column: "project_id",
                unique: true);
        }
    }
}
