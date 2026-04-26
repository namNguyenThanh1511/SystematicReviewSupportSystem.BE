using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DeduplicationInProjectScoped : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_deduplication_results_identification_processes_identificati~",
                table: "deduplication_results");

            migrationBuilder.RenameColumn(
                name: "identification_process_id",
                table: "deduplication_results",
                newName: "project_id");

            migrationBuilder.RenameIndex(
                name: "uq_deduplication_process_paper",
                table: "deduplication_results",
                newName: "uq_deduplication_project_paper");

            migrationBuilder.RenameIndex(
                name: "IX_deduplication_results_identification_process_id",
                table: "deduplication_results",
                newName: "IX_deduplication_results_project_id");

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "papers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "confidence_score",
                table: "deduplication_results",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,4)",
                oldPrecision: 5,
                oldScale: 4,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_deduplication_results_systematic_review_projects_project_id",
                table: "deduplication_results",
                column: "project_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_deduplication_results_systematic_review_projects_project_id",
                table: "deduplication_results");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "papers");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "deduplication_results",
                newName: "identification_process_id");

            migrationBuilder.RenameIndex(
                name: "uq_deduplication_project_paper",
                table: "deduplication_results",
                newName: "uq_deduplication_process_paper");

            migrationBuilder.RenameIndex(
                name: "IX_deduplication_results_project_id",
                table: "deduplication_results",
                newName: "IX_deduplication_results_identification_process_id");

            migrationBuilder.AlterColumn<decimal>(
                name: "confidence_score",
                table: "deduplication_results",
                type: "numeric(5,4)",
                precision: 5,
                scale: 4,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,4)",
                oldPrecision: 5,
                oldScale: 4);

            migrationBuilder.AddForeignKey(
                name: "FK_deduplication_results_identification_processes_identificati~",
                table: "deduplication_results",
                column: "identification_process_id",
                principalTable: "identification_processes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
