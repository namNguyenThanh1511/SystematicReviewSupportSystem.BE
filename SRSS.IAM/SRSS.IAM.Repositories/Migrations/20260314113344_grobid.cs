using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class grobid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GrobidHeaderResults_PaperPdfs_PaperPdfId",
                table: "GrobidHeaderResults");

            migrationBuilder.DropForeignKey(
                name: "FK_PaperPdfs_papers_PaperId",
                table: "PaperPdfs");

            migrationBuilder.DropForeignKey(
                name: "FK_PaperSourceMetadatas_papers_PaperId",
                table: "PaperSourceMetadatas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaperSourceMetadatas",
                table: "PaperSourceMetadatas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaperPdfs",
                table: "PaperPdfs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GrobidHeaderResults",
                table: "GrobidHeaderResults");

            migrationBuilder.RenameTable(
                name: "PaperSourceMetadatas",
                newName: "paper_source_metadatas");

            migrationBuilder.RenameTable(
                name: "PaperPdfs",
                newName: "paper_pdfs");

            migrationBuilder.RenameTable(
                name: "GrobidHeaderResults",
                newName: "grobid_header_results");

            migrationBuilder.RenameIndex(
                name: "IX_PaperSourceMetadatas_PaperId",
                table: "paper_source_metadatas",
                newName: "IX_paper_source_metadatas_PaperId");

            migrationBuilder.RenameIndex(
                name: "IX_PaperPdfs_PaperId",
                table: "paper_pdfs",
                newName: "IX_paper_pdfs_PaperId");

            migrationBuilder.RenameIndex(
                name: "IX_GrobidHeaderResults_PaperPdfId",
                table: "grobid_header_results",
                newName: "IX_grobid_header_results_PaperPdfId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_paper_source_metadatas",
                table: "paper_source_metadatas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_paper_pdfs",
                table: "paper_pdfs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_grobid_header_results",
                table: "grobid_header_results",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_grobid_header_results_paper_pdfs_PaperPdfId",
                table: "grobid_header_results",
                column: "PaperPdfId",
                principalTable: "paper_pdfs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_paper_pdfs_papers_PaperId",
                table: "paper_pdfs",
                column: "PaperId",
                principalTable: "papers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_paper_source_metadatas_papers_PaperId",
                table: "paper_source_metadatas",
                column: "PaperId",
                principalTable: "papers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_grobid_header_results_paper_pdfs_PaperPdfId",
                table: "grobid_header_results");

            migrationBuilder.DropForeignKey(
                name: "FK_paper_pdfs_papers_PaperId",
                table: "paper_pdfs");

            migrationBuilder.DropForeignKey(
                name: "FK_paper_source_metadatas_papers_PaperId",
                table: "paper_source_metadatas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_paper_source_metadatas",
                table: "paper_source_metadatas");

            migrationBuilder.DropPrimaryKey(
                name: "PK_paper_pdfs",
                table: "paper_pdfs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_grobid_header_results",
                table: "grobid_header_results");

            migrationBuilder.RenameTable(
                name: "paper_source_metadatas",
                newName: "PaperSourceMetadatas");

            migrationBuilder.RenameTable(
                name: "paper_pdfs",
                newName: "PaperPdfs");

            migrationBuilder.RenameTable(
                name: "grobid_header_results",
                newName: "GrobidHeaderResults");

            migrationBuilder.RenameIndex(
                name: "IX_paper_source_metadatas_PaperId",
                table: "PaperSourceMetadatas",
                newName: "IX_PaperSourceMetadatas_PaperId");

            migrationBuilder.RenameIndex(
                name: "IX_paper_pdfs_PaperId",
                table: "PaperPdfs",
                newName: "IX_PaperPdfs_PaperId");

            migrationBuilder.RenameIndex(
                name: "IX_grobid_header_results_PaperPdfId",
                table: "GrobidHeaderResults",
                newName: "IX_GrobidHeaderResults_PaperPdfId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaperSourceMetadatas",
                table: "PaperSourceMetadatas",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaperPdfs",
                table: "PaperPdfs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GrobidHeaderResults",
                table: "GrobidHeaderResults",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GrobidHeaderResults_PaperPdfs_PaperPdfId",
                table: "GrobidHeaderResults",
                column: "PaperPdfId",
                principalTable: "PaperPdfs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaperPdfs_papers_PaperId",
                table: "PaperPdfs",
                column: "PaperId",
                principalTable: "papers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PaperSourceMetadatas_papers_PaperId",
                table: "PaperSourceMetadatas",
                column: "PaperId",
                principalTable: "papers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
