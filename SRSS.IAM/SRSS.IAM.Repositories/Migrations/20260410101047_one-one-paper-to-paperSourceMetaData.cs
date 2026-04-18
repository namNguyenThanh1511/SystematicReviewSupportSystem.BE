using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class oneonepapertopaperSourceMetaData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_paper_source_metadatas_PaperId",
                table: "paper_source_metadatas");

            migrationBuilder.CreateIndex(
                name: "IX_paper_source_metadatas_PaperId",
                table: "paper_source_metadatas",
                column: "PaperId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_paper_source_metadatas_PaperId",
                table: "paper_source_metadatas");

            migrationBuilder.CreateIndex(
                name: "IX_paper_source_metadatas_PaperId",
                table: "paper_source_metadatas",
                column: "PaperId");
        }
    }
}
