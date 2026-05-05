using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddPaperPdfPageSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "PageHeight",
                table: "paper_pdfs",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PageWidth",
                table: "paper_pdfs",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PageHeight",
                table: "paper_pdfs");

            migrationBuilder.DropColumn(
                name: "PageWidth",
                table: "paper_pdfs");
        }
    }
}
