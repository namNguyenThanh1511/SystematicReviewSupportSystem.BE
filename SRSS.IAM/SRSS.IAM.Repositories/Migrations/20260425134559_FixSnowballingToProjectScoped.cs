using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class FixSnowballingToProjectScoped : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_candidate_papers_review_processes_ReviewProcessId",
                table: "candidate_papers");

            migrationBuilder.DropIndex(
                name: "IX_candidate_papers_ReviewProcessId",
                table: "candidate_papers");

            migrationBuilder.DropColumn(
                name: "ReviewProcessId",
                table: "candidate_papers");

            migrationBuilder.RenameColumn(
                name: "IsSelectedInScreening",
                table: "candidate_papers",
                newName: "IsSelectedInProjectRepository");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSelectedInProjectRepository",
                table: "candidate_papers",
                newName: "IsSelectedInScreening");

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewProcessId",
                table: "candidate_papers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_candidate_papers_ReviewProcessId",
                table: "candidate_papers",
                column: "ReviewProcessId");

            migrationBuilder.AddForeignKey(
                name: "FK_candidate_papers_review_processes_ReviewProcessId",
                table: "candidate_papers",
                column: "ReviewProcessId",
                principalTable: "review_processes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
