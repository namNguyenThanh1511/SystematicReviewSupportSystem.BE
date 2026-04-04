using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class removerefsentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_candidate_papers_reference_entities_ReferenceEntityId",
                table: "candidate_papers");

            migrationBuilder.DropForeignKey(
                name: "FK_paper_citations_reference_entities_reference_entity_id",
                table: "paper_citations");

            migrationBuilder.DropTable(
                name: "reference_entities");

            migrationBuilder.DropIndex(
                name: "IX_paper_citations_reference_entity_id",
                table: "paper_citations");

            migrationBuilder.DropIndex(
                name: "IX_candidate_papers_ReferenceEntityId",
                table: "candidate_papers");

            migrationBuilder.DropColumn(
                name: "ReferenceEntityId",
                table: "candidate_papers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceEntityId",
                table: "candidate_papers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "reference_entities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    authors = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    doi = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    raw_reference = table.Column<string>(type: "text", nullable: true),
                    title = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    type = table.Column<string>(type: "text", nullable: false),
                    url = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reference_entities", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_paper_citations_reference_entity_id",
                table: "paper_citations",
                column: "reference_entity_id");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_papers_ReferenceEntityId",
                table: "candidate_papers",
                column: "ReferenceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_reference_entities_doi",
                table: "reference_entities",
                column: "doi");

            migrationBuilder.CreateIndex(
                name: "IX_reference_entities_type",
                table: "reference_entities",
                column: "type");

            migrationBuilder.AddForeignKey(
                name: "FK_candidate_papers_reference_entities_ReferenceEntityId",
                table: "candidate_papers",
                column: "ReferenceEntityId",
                principalTable: "reference_entities",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_paper_citations_reference_entities_reference_entity_id",
                table: "paper_citations",
                column: "reference_entity_id",
                principalTable: "reference_entities",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
