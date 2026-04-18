using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RemoveChecklistContentAndIsNotApplicable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "content",
                table: "checklist_item_responses");

            migrationBuilder.DropColumn(
                name: "is_not_applicable",
                table: "checklist_item_responses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "content",
                table: "checklist_item_responses",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_not_applicable",
                table: "checklist_item_responses",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
