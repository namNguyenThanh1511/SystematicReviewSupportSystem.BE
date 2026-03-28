using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class removereviewprocessidpaper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_papers_review_processes_review_process_id",
                table: "papers");

            migrationBuilder.RenameColumn(
                name: "review_process_id",
                table: "papers",
                newName: "ReviewProcessId");

            migrationBuilder.RenameIndex(
                name: "IX_papers_review_process_id",
                table: "papers",
                newName: "IX_papers_ReviewProcessId");

            migrationBuilder.AddForeignKey(
                name: "FK_papers_review_processes_ReviewProcessId",
                table: "papers",
                column: "ReviewProcessId",
                principalTable: "review_processes",
                principalColumn: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_papers_review_processes_ReviewProcessId",
                table: "papers");

            migrationBuilder.RenameColumn(
                name: "ReviewProcessId",
                table: "papers",
                newName: "review_process_id");

            migrationBuilder.RenameIndex(
                name: "IX_papers_ReviewProcessId",
                table: "papers",
                newName: "IX_papers_review_process_id");

            migrationBuilder.AddForeignKey(
                name: "FK_papers_review_processes_review_process_id",
                table: "papers",
                column: "review_process_id",
                principalTable: "review_processes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
