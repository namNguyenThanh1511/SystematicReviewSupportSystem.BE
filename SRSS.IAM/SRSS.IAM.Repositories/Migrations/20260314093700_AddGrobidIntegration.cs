using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddGrobidIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaperPdfs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaperId = table.Column<Guid>(type: "uuid", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GrobidProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperPdfs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaperPdfs_papers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaperSourceMetadatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaperId = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Authors = table.Column<string>(type: "text", nullable: true),
                    Abstract = table.Column<string>(type: "text", nullable: true),
                    DOI = table.Column<string>(type: "text", nullable: true),
                    Journal = table.Column<string>(type: "text", nullable: true),
                    Volume = table.Column<string>(type: "text", nullable: true),
                    Issue = table.Column<string>(type: "text", nullable: true),
                    Pages = table.Column<string>(type: "text", nullable: true),
                    Keywords = table.Column<string>(type: "text", nullable: true),
                    ExtractedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaperSourceMetadatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaperSourceMetadatas_papers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GrobidHeaderResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaperPdfId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Authors = table.Column<string>(type: "text", nullable: true),
                    Abstract = table.Column<string>(type: "text", nullable: true),
                    DOI = table.Column<string>(type: "text", nullable: true),
                    Journal = table.Column<string>(type: "text", nullable: true),
                    Volume = table.Column<string>(type: "text", nullable: true),
                    Issue = table.Column<string>(type: "text", nullable: true),
                    Pages = table.Column<string>(type: "text", nullable: true),
                    RawXml = table.Column<string>(type: "text", nullable: true),
                    ExtractedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GrobidHeaderResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GrobidHeaderResults_PaperPdfs_PaperPdfId",
                        column: x => x.PaperPdfId,
                        principalTable: "PaperPdfs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GrobidHeaderResults_PaperPdfId",
                table: "GrobidHeaderResults",
                column: "PaperPdfId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaperPdfs_PaperId",
                table: "PaperPdfs",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_PaperSourceMetadatas_PaperId",
                table: "PaperSourceMetadatas",
                column: "PaperId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GrobidHeaderResults");

            migrationBuilder.DropTable(
                name: "PaperSourceMetadatas");

            migrationBuilder.DropTable(
                name: "PaperPdfs");
        }
    }
}
