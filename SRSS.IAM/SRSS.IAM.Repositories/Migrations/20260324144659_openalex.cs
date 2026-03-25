using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class openalex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_paper_citations_papers_SourcePaperId",
                table: "paper_citations");

            migrationBuilder.DropForeignKey(
                name: "FK_paper_citations_papers_TargetPaperId",
                table: "paper_citations");

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "paper_citations",
                newName: "source");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "paper_citations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TargetPaperId",
                table: "paper_citations",
                newName: "target_paper_id");

            migrationBuilder.RenameColumn(
                name: "SourcePaperId",
                table: "paper_citations",
                newName: "source_paper_id");

            migrationBuilder.RenameColumn(
                name: "RawReference",
                table: "paper_citations",
                newName: "raw_reference");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "paper_citations",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "paper_citations",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ConfidenceScore",
                table: "paper_citations",
                newName: "confidence_score");

            migrationBuilder.RenameIndex(
                name: "IX_paper_citations_TargetPaperId",
                table: "paper_citations",
                newName: "IX_paper_citations_target_paper_id");

            migrationBuilder.RenameIndex(
                name: "IX_paper_citations_SourcePaperId_TargetPaperId",
                table: "paper_citations",
                newName: "IX_paper_citations_source_paper_id_target_paper_id");

            migrationBuilder.RenameIndex(
                name: "IX_paper_citations_SourcePaperId",
                table: "paper_citations",
                newName: "IX_paper_citations_source_paper_id");

            migrationBuilder.AddColumn<int>(
                name: "external_citation_count",
                table: "papers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "external_cited_by_percentile",
                table: "papers",
                type: "double precision",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "external_data_fetched",
                table: "papers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "external_last_fetched_at",
                table: "papers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "external_reference_count",
                table: "papers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_source",
                table: "papers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "open_alex_id",
                table: "papers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "source",
                table: "paper_citations",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "external_id",
                table: "paper_citations",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_external",
                table: "paper_citations",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "weight",
                table: "paper_citations",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_paper_citations_papers_source_paper_id",
                table: "paper_citations",
                column: "source_paper_id",
                principalTable: "papers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_paper_citations_papers_target_paper_id",
                table: "paper_citations",
                column: "target_paper_id",
                principalTable: "papers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_paper_citations_papers_source_paper_id",
                table: "paper_citations");

            migrationBuilder.DropForeignKey(
                name: "FK_paper_citations_papers_target_paper_id",
                table: "paper_citations");

            migrationBuilder.DropColumn(
                name: "external_citation_count",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "external_cited_by_percentile",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "external_data_fetched",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "external_last_fetched_at",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "external_reference_count",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "external_source",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "open_alex_id",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "external_id",
                table: "paper_citations");

            migrationBuilder.DropColumn(
                name: "is_external",
                table: "paper_citations");

            migrationBuilder.DropColumn(
                name: "weight",
                table: "paper_citations");

            migrationBuilder.RenameColumn(
                name: "source",
                table: "paper_citations",
                newName: "Source");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "paper_citations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "target_paper_id",
                table: "paper_citations",
                newName: "TargetPaperId");

            migrationBuilder.RenameColumn(
                name: "source_paper_id",
                table: "paper_citations",
                newName: "SourcePaperId");

            migrationBuilder.RenameColumn(
                name: "raw_reference",
                table: "paper_citations",
                newName: "RawReference");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "paper_citations",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "paper_citations",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "confidence_score",
                table: "paper_citations",
                newName: "ConfidenceScore");

            migrationBuilder.RenameIndex(
                name: "IX_paper_citations_target_paper_id",
                table: "paper_citations",
                newName: "IX_paper_citations_TargetPaperId");

            migrationBuilder.RenameIndex(
                name: "IX_paper_citations_source_paper_id_target_paper_id",
                table: "paper_citations",
                newName: "IX_paper_citations_SourcePaperId_TargetPaperId");

            migrationBuilder.RenameIndex(
                name: "IX_paper_citations_source_paper_id",
                table: "paper_citations",
                newName: "IX_paper_citations_SourcePaperId");

            migrationBuilder.AlterColumn<int>(
                name: "Source",
                table: "paper_citations",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "FK_paper_citations_papers_SourcePaperId",
                table: "paper_citations",
                column: "SourcePaperId",
                principalTable: "papers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_paper_citations_papers_TargetPaperId",
                table: "paper_citations",
                column: "TargetPaperId",
                principalTable: "papers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
