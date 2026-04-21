using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RefactorUploadingRis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_search_executions_identification_processes_identification_p~",
                table: "search_executions");

            migrationBuilder.AlterColumn<Guid>(
                name: "identification_process_id",
                table: "search_executions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "project_id",
                table: "search_executions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "created_from_import_batch_id",
                table: "papers",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "identification_process_search_strategies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identification_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    search_strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identification_process_search_strategies", x => x.id);
                    table.ForeignKey(
                        name: "FK_identification_process_search_strategies_identification_pro~",
                        column: x => x.identification_process_id,
                        principalTable: "identification_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_identification_process_search_strategies_search_executions_~",
                        column: x => x.search_strategy_id,
                        principalTable: "search_executions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_search_executions_project_id",
                table: "search_executions",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_identification_process_search_strategies_identification_pro~",
                table: "identification_process_search_strategies",
                columns: new[] { "identification_process_id", "search_strategy_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_identification_process_search_strategies_search_strategy_id",
                table: "identification_process_search_strategies",
                column: "search_strategy_id");

            migrationBuilder.AddForeignKey(
                name: "FK_search_executions_identification_processes_identification_p~",
                table: "search_executions",
                column: "identification_process_id",
                principalTable: "identification_processes",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_search_executions_systematic_review_projects_project_id",
                table: "search_executions",
                column: "project_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_search_executions_identification_processes_identification_p~",
                table: "search_executions");

            migrationBuilder.DropForeignKey(
                name: "FK_search_executions_systematic_review_projects_project_id",
                table: "search_executions");

            migrationBuilder.DropTable(
                name: "identification_process_search_strategies");

            migrationBuilder.DropIndex(
                name: "IX_search_executions_project_id",
                table: "search_executions");

            migrationBuilder.DropColumn(
                name: "project_id",
                table: "search_executions");

            migrationBuilder.DropColumn(
                name: "created_from_import_batch_id",
                table: "papers");

            migrationBuilder.AlterColumn<Guid>(
                name: "identification_process_id",
                table: "search_executions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_search_executions_identification_processes_identification_p~",
                table: "search_executions",
                column: "identification_process_id",
                principalTable: "identification_processes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
