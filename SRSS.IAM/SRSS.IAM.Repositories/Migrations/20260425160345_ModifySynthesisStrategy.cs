using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class ModifySynthesisStrategy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_data_synthesis_strategy_systematic_review_projects_project_~",
                table: "data_synthesis_strategy");

            migrationBuilder.Sql("DELETE FROM data_synthesis_strategy;");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "data_synthesis_strategy",
                newName: "synthesis_process_id");

            migrationBuilder.RenameIndex(
                name: "IX_data_synthesis_strategy_project_id",
                table: "data_synthesis_strategy",
                newName: "IX_data_synthesis_strategy_synthesis_process_id");

            migrationBuilder.AddForeignKey(
                name: "FK_data_synthesis_strategy_synthesis_processes_synthesis_proce~",
                table: "data_synthesis_strategy",
                column: "synthesis_process_id",
                principalTable: "synthesis_processes",
                principalColumn: "synthesis_process_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_data_synthesis_strategy_synthesis_processes_synthesis_proce~",
                table: "data_synthesis_strategy");

            migrationBuilder.RenameColumn(
                name: "synthesis_process_id",
                table: "data_synthesis_strategy",
                newName: "project_id");

            migrationBuilder.RenameIndex(
                name: "IX_data_synthesis_strategy_synthesis_process_id",
                table: "data_synthesis_strategy",
                newName: "IX_data_synthesis_strategy_project_id");

            migrationBuilder.AddForeignKey(
                name: "FK_data_synthesis_strategy_systematic_review_projects_project_~",
                table: "data_synthesis_strategy",
                column: "project_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
