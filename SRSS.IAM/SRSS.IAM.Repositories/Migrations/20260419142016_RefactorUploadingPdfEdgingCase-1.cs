using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RefactorUploadingPdfEdgingCase1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ValidationStatus",
                table: "paper_pdfs",
                type: "text",
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessingStatus",
                table: "paper_pdfs",
                type: "text",
                nullable: false,
                defaultValue: "Uploaded",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "ValidationStatus",
                table: "paper_pdfs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Pending");

            migrationBuilder.AlterColumn<int>(
                name: "ProcessingStatus",
                table: "paper_pdfs",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Uploaded");
        }
    }
}
