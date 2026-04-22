using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RefactorUploadingPdfEdgingCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FileHash",
                table: "paper_pdfs",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExtractedDoi",
                table: "paper_pdfs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FullTextProcessedAt",
                table: "paper_pdfs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MetadataProcessedAt",
                table: "paper_pdfs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "MetadataValidatedAt",
                table: "paper_pdfs",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProcessingStatus",
                table: "paper_pdfs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ValidationStatus",
                table: "paper_pdfs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtractedDoi",
                table: "paper_pdfs");

            migrationBuilder.DropColumn(
                name: "FullTextProcessedAt",
                table: "paper_pdfs");

            migrationBuilder.DropColumn(
                name: "MetadataProcessedAt",
                table: "paper_pdfs");

            migrationBuilder.DropColumn(
                name: "MetadataValidatedAt",
                table: "paper_pdfs");

            migrationBuilder.DropColumn(
                name: "ProcessingStatus",
                table: "paper_pdfs");

            migrationBuilder.DropColumn(
                name: "ValidationStatus",
                table: "paper_pdfs");

            migrationBuilder.AlterColumn<string>(
                name: "FileHash",
                table: "paper_pdfs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);
        }
    }
}
