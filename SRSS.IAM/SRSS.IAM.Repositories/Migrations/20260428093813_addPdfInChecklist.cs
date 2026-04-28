using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class addPdfInChecklist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsCompleted",
                table: "checklist_item_responses",
                newName: "is_completed");

            migrationBuilder.AddColumn<string>(
                name: "pdf_url",
                table: "review_checklists",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pdf_coordinates",
                table: "checklist_item_responses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "pdf_url",
                table: "review_checklists");

            migrationBuilder.DropColumn(
                name: "pdf_coordinates",
                table: "checklist_item_responses");

            migrationBuilder.RenameColumn(
                name: "is_completed",
                table: "checklist_item_responses",
                newName: "IsCompleted");
        }
    }
}
