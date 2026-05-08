using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class PdfSizeInChecklist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "page_height",
                table: "review_checklists",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "page_width",
                table: "review_checklists",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "page_height",
                table: "review_checklists");

            migrationBuilder.DropColumn(
                name: "page_width",
                table: "review_checklists");
        }
    }
}
