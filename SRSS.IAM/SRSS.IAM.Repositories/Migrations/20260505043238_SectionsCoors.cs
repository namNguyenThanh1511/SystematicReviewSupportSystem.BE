using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class SectionsCoors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Coordinates",
                table: "paper_full_text_parsed_sections",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Coordinates",
                table: "paper_full_text_parsed_paragraphs",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Coordinates",
                table: "paper_full_text_parsed_sections");

            migrationBuilder.DropColumn(
                name: "Coordinates",
                table: "paper_full_text_parsed_paragraphs");
        }
    }
}
