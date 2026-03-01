using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRemovedAsDuplicateToPaper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_removed_as_duplicate",
                table: "papers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_papers_is_removed_as_duplicate",
                table: "papers",
                column: "is_removed_as_duplicate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_papers_is_removed_as_duplicate",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "is_removed_as_duplicate",
                table: "papers");
        }
    }
}
