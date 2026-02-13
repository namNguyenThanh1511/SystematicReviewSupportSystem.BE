using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class importbatch1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_papers_search_executions_search_execution_id",
                table: "papers");

            migrationBuilder.DropIndex(
                name: "IX_papers_search_execution_id",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "search_execution_id",
                table: "papers");

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_search_execution_id",
                table: "import_batches",
                column: "search_execution_id");

            migrationBuilder.AddForeignKey(
                name: "FK_import_batches_search_executions_search_execution_id",
                table: "import_batches",
                column: "search_execution_id",
                principalTable: "search_executions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_import_batches_search_executions_search_execution_id",
                table: "import_batches");

            migrationBuilder.DropIndex(
                name: "IX_import_batches_search_execution_id",
                table: "import_batches");

            migrationBuilder.AddColumn<Guid>(
                name: "search_execution_id",
                table: "papers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_papers_search_execution_id",
                table: "papers",
                column: "search_execution_id");

            migrationBuilder.AddForeignKey(
                name: "FK_papers_search_executions_search_execution_id",
                table: "papers",
                column: "search_execution_id",
                principalTable: "search_executions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
