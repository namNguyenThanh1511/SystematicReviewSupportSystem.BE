using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddPrisma2020Checklist1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_review_checklists_systematic_review_projects_review_id",
                table: "review_checklists");

            migrationBuilder.RenameColumn(
                name: "review_id",
                table: "review_checklists",
                newName: "project_id");

            migrationBuilder.RenameIndex(
                name: "ux_review_checklists_review_template",
                table: "review_checklists",
                newName: "ux_review_checklists_project_template");

            migrationBuilder.RenameIndex(
                name: "idx_review_checklists_review_id",
                table: "review_checklists",
                newName: "idx_review_checklists_project_id");

            migrationBuilder.AddForeignKey(
                name: "FK_review_checklists_systematic_review_projects_project_id",
                table: "review_checklists",
                column: "project_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_review_checklists_systematic_review_projects_project_id",
                table: "review_checklists");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "review_checklists",
                newName: "review_id");

            migrationBuilder.RenameIndex(
                name: "ux_review_checklists_project_template",
                table: "review_checklists",
                newName: "ux_review_checklists_review_template");

            migrationBuilder.RenameIndex(
                name: "idx_review_checklists_project_id",
                table: "review_checklists",
                newName: "idx_review_checklists_review_id");

            migrationBuilder.AddForeignKey(
                name: "FK_review_checklists_systematic_review_projects_review_id",
                table: "review_checklists",
                column: "review_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
