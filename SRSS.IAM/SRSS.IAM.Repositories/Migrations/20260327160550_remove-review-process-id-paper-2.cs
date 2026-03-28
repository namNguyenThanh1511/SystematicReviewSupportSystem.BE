using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class removereviewprocessidpaper2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CitationId",
                table: "candidate_papers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSelectedInScreening",
                table: "candidate_papers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ReferenceEntityId",
                table: "candidate_papers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "SelectedAt",
                table: "candidate_papers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetPaperId",
                table: "candidate_papers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_candidate_papers_CitationId",
                table: "candidate_papers",
                column: "CitationId");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_papers_ReferenceEntityId",
                table: "candidate_papers",
                column: "ReferenceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_candidate_papers_TargetPaperId",
                table: "candidate_papers",
                column: "TargetPaperId");

            migrationBuilder.AddForeignKey(
                name: "FK_candidate_papers_paper_citations_CitationId",
                table: "candidate_papers",
                column: "CitationId",
                principalTable: "paper_citations",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_candidate_papers_papers_TargetPaperId",
                table: "candidate_papers",
                column: "TargetPaperId",
                principalTable: "papers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_candidate_papers_reference_entities_ReferenceEntityId",
                table: "candidate_papers",
                column: "ReferenceEntityId",
                principalTable: "reference_entities",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_candidate_papers_paper_citations_CitationId",
                table: "candidate_papers");

            migrationBuilder.DropForeignKey(
                name: "FK_candidate_papers_papers_TargetPaperId",
                table: "candidate_papers");

            migrationBuilder.DropForeignKey(
                name: "FK_candidate_papers_reference_entities_ReferenceEntityId",
                table: "candidate_papers");

            migrationBuilder.DropIndex(
                name: "IX_candidate_papers_CitationId",
                table: "candidate_papers");

            migrationBuilder.DropIndex(
                name: "IX_candidate_papers_ReferenceEntityId",
                table: "candidate_papers");

            migrationBuilder.DropIndex(
                name: "IX_candidate_papers_TargetPaperId",
                table: "candidate_papers");

            migrationBuilder.DropColumn(
                name: "CitationId",
                table: "candidate_papers");

            migrationBuilder.DropColumn(
                name: "IsSelectedInScreening",
                table: "candidate_papers");

            migrationBuilder.DropColumn(
                name: "ReferenceEntityId",
                table: "candidate_papers");

            migrationBuilder.DropColumn(
                name: "SelectedAt",
                table: "candidate_papers");

            migrationBuilder.DropColumn(
                name: "TargetPaperId",
                table: "candidate_papers");
        }
    }
}
