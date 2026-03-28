using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class removereviewprocessidpaper1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_papers_review_processes_ReviewProcessId",
                table: "papers");

            migrationBuilder.DropIndex(
                name: "IX_papers_ReviewProcessId",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "ReviewProcessId",
                table: "papers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReviewProcessId",
                table: "papers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_papers_ReviewProcessId",
                table: "papers",
                column: "ReviewProcessId");

            migrationBuilder.AddForeignKey(
                name: "FK_papers_review_processes_ReviewProcessId",
                table: "papers",
                column: "ReviewProcessId",
                principalTable: "review_processes",
                principalColumn: "id");
        }
    }
}
