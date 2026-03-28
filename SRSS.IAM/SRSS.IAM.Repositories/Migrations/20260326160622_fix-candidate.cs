using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class fixcandidate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidatePapers_papers_OriginPaperId",
                table: "CandidatePapers");

            migrationBuilder.DropForeignKey(
                name: "FK_CandidatePapers_review_processes_ReviewProcessId",
                table: "CandidatePapers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CandidatePapers",
                table: "CandidatePapers");

            migrationBuilder.RenameTable(
                name: "CandidatePapers",
                newName: "candidate_papers");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "candidate_papers",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "DOI",
                table: "candidate_papers",
                newName: "doi");

            migrationBuilder.RenameColumn(
                name: "Authors",
                table: "candidate_papers",
                newName: "authors");

            migrationBuilder.RenameColumn(
                name: "PublicationYear",
                table: "candidate_papers",
                newName: "publication_year");

            migrationBuilder.RenameIndex(
                name: "IX_CandidatePapers_ReviewProcessId",
                table: "candidate_papers",
                newName: "IX_candidate_papers_ReviewProcessId");

            migrationBuilder.RenameIndex(
                name: "IX_CandidatePapers_OriginPaperId",
                table: "candidate_papers",
                newName: "IX_candidate_papers_OriginPaperId");

            migrationBuilder.AddColumn<decimal>(
                name: "confidence_score",
                table: "candidate_papers",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddPrimaryKey(
                name: "id",
                table: "candidate_papers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_candidate_papers_papers_OriginPaperId",
                table: "candidate_papers",
                column: "OriginPaperId",
                principalTable: "papers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_candidate_papers_review_processes_ReviewProcessId",
                table: "candidate_papers",
                column: "ReviewProcessId",
                principalTable: "review_processes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_candidate_papers_papers_OriginPaperId",
                table: "candidate_papers");

            migrationBuilder.DropForeignKey(
                name: "FK_candidate_papers_review_processes_ReviewProcessId",
                table: "candidate_papers");

            migrationBuilder.DropPrimaryKey(
                name: "id",
                table: "candidate_papers");

            migrationBuilder.DropColumn(
                name: "confidence_score",
                table: "candidate_papers");

            migrationBuilder.RenameTable(
                name: "candidate_papers",
                newName: "CandidatePapers");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "CandidatePapers",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "doi",
                table: "CandidatePapers",
                newName: "DOI");

            migrationBuilder.RenameColumn(
                name: "authors",
                table: "CandidatePapers",
                newName: "Authors");

            migrationBuilder.RenameColumn(
                name: "publication_year",
                table: "CandidatePapers",
                newName: "PublicationYear");

            migrationBuilder.RenameIndex(
                name: "IX_candidate_papers_ReviewProcessId",
                table: "CandidatePapers",
                newName: "IX_CandidatePapers_ReviewProcessId");

            migrationBuilder.RenameIndex(
                name: "IX_candidate_papers_OriginPaperId",
                table: "CandidatePapers",
                newName: "IX_CandidatePapers_OriginPaperId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CandidatePapers",
                table: "CandidatePapers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CandidatePapers_papers_OriginPaperId",
                table: "CandidatePapers",
                column: "OriginPaperId",
                principalTable: "papers",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CandidatePapers_review_processes_ReviewProcessId",
                table: "CandidatePapers",
                column: "ReviewProcessId",
                principalTable: "review_processes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
