using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddMd5AndExtraGrobidFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Md5",
                table: "papers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EISSN",
                table: "paper_source_metadatas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ISSN",
                table: "paper_source_metadatas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "paper_source_metadatas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Md5",
                table: "paper_source_metadatas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublishedDate",
                table: "paper_source_metadatas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Publisher",
                table: "paper_source_metadatas",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "paper_source_metadatas",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Md5",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "EISSN",
                table: "paper_source_metadatas");

            migrationBuilder.DropColumn(
                name: "ISSN",
                table: "paper_source_metadatas");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "paper_source_metadatas");

            migrationBuilder.DropColumn(
                name: "Md5",
                table: "paper_source_metadatas");

            migrationBuilder.DropColumn(
                name: "PublishedDate",
                table: "paper_source_metadatas");

            migrationBuilder.DropColumn(
                name: "Publisher",
                table: "paper_source_metadatas");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "paper_source_metadatas");
        }
    }
}
