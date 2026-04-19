using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class fulltextpaperchunking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ChunkedAt",
                table: "paper_full_texts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "EmbeddedAt",
                table: "paper_full_texts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ParsedAt",
                table: "paper_full_texts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "paper_full_text_chunks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaperFullTextId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    SectionTitle = table.Column<string>(type: "text", nullable: false),
                    SectionType = table.Column<string>(type: "text", nullable: true),
                    Text = table.Column<string>(type: "text", nullable: false),
                    WordCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paper_full_text_chunks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_paper_full_text_chunks_paper_full_texts_PaperFullTextId",
                        column: x => x.PaperFullTextId,
                        principalTable: "paper_full_texts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "paper_full_text_parsed_sections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaperFullTextId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    SectionTitle = table.Column<string>(type: "text", nullable: false),
                    SectionType = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paper_full_text_parsed_sections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_paper_full_text_parsed_sections_paper_full_texts_PaperFullT~",
                        column: x => x.PaperFullTextId,
                        principalTable: "paper_full_texts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "paper_full_text_chunk_embeddings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChunkId = table.Column<Guid>(type: "uuid", nullable: false),
                    ModelName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Vector = table.Column<Vector>(type: "vector", nullable: false),
                    EmbeddedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paper_full_text_chunk_embeddings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_paper_full_text_chunk_embeddings_paper_full_text_chunks_Chu~",
                        column: x => x.ChunkId,
                        principalTable: "paper_full_text_chunks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "paper_full_text_parsed_paragraphs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    Text = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paper_full_text_parsed_paragraphs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_paper_full_text_parsed_paragraphs_paper_full_text_parsed_se~",
                        column: x => x.SectionId,
                        principalTable: "paper_full_text_parsed_sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_paper_full_text_chunk_embeddings_ChunkId",
                table: "paper_full_text_chunk_embeddings",
                column: "ChunkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_paper_full_text_chunks_paper_id_order",
                table: "paper_full_text_chunks",
                columns: new[] { "PaperFullTextId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_paper_full_text_parsed_paragraphs_section_id_order",
                table: "paper_full_text_parsed_paragraphs",
                columns: new[] { "SectionId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_paper_full_text_parsed_sections_paper_id_order",
                table: "paper_full_text_parsed_sections",
                columns: new[] { "PaperFullTextId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "paper_full_text_chunk_embeddings");

            migrationBuilder.DropTable(
                name: "paper_full_text_parsed_paragraphs");

            migrationBuilder.DropTable(
                name: "paper_full_text_chunks");

            migrationBuilder.DropTable(
                name: "paper_full_text_parsed_sections");

            migrationBuilder.DropColumn(
                name: "ChunkedAt",
                table: "paper_full_texts");

            migrationBuilder.DropColumn(
                name: "EmbeddedAt",
                table: "paper_full_texts");

            migrationBuilder.DropColumn(
                name: "ParsedAt",
                table: "paper_full_texts");
        }
    }
}
