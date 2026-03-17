using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class candidate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidatePapers_systematic_review_projects_ProjectId",
                table: "CandidatePapers");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "CandidatePapers",
                newName: "ReviewProcessId");

            migrationBuilder.RenameIndex(
                name: "IX_CandidatePapers_ProjectId",
                table: "CandidatePapers",
                newName: "IX_CandidatePapers_ReviewProcessId");

            migrationBuilder.AddColumn<Guid>(
                name: "review_process_id",
                table: "papers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_papers_review_process_id",
                table: "papers",
                column: "review_process_id");

            migrationBuilder.AddForeignKey(
                name: "FK_CandidatePapers_review_processes_ReviewProcessId",
                table: "CandidatePapers",
                column: "ReviewProcessId",
                principalTable: "review_processes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_papers_review_processes_review_process_id",
                table: "papers",
                column: "review_process_id",
                principalTable: "review_processes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidatePapers_review_processes_ReviewProcessId",
                table: "CandidatePapers");

            migrationBuilder.DropForeignKey(
                name: "FK_papers_review_processes_review_process_id",
                table: "papers");

            migrationBuilder.DropIndex(
                name: "IX_papers_review_process_id",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "review_process_id",
                table: "papers");

            migrationBuilder.RenameColumn(
                name: "ReviewProcessId",
                table: "CandidatePapers",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_CandidatePapers_ReviewProcessId",
                table: "CandidatePapers",
                newName: "IX_CandidatePapers_ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_CandidatePapers_systematic_review_projects_ProjectId",
                table: "CandidatePapers",
                column: "ProjectId",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
