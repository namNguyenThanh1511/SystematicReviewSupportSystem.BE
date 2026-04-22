using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class DeduplicationInProjectScoped1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_import_batches_search_executions_search_execution_id",
                table: "import_batches");

            migrationBuilder.DropTable(
                name: "identification_process_search_strategies");

            migrationBuilder.DropTable(
                name: "search_executions");

            migrationBuilder.DropIndex(
                name: "IX_import_batches_search_execution_id",
                table: "import_batches");

            migrationBuilder.DropColumn(
                name: "search_execution_id",
                table: "import_batches");

            migrationBuilder.AddColumn<Guid>(
                name: "project_id",
                table: "import_batches",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_project_id",
                table: "import_batches",
                column: "project_id");

            migrationBuilder.AddForeignKey(
                name: "FK_import_batches_systematic_review_projects_project_id",
                table: "import_batches",
                column: "project_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_import_batches_systematic_review_projects_project_id",
                table: "import_batches");

            migrationBuilder.DropIndex(
                name: "IX_import_batches_project_id",
                table: "import_batches");

            migrationBuilder.DropColumn(
                name: "project_id",
                table: "import_batches");

            migrationBuilder.AddColumn<Guid>(
                name: "search_execution_id",
                table: "import_batches",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "search_executions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identification_process_id = table.Column<Guid>(type: "uuid", nullable: true),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    search_source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    executed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    result_count = table.Column<int>(type: "integer", nullable: false),
                    search_query = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_executions", x => x.id);
                    table.ForeignKey(
                        name: "FK_search_executions_identification_processes_identification_p~",
                        column: x => x.identification_process_id,
                        principalTable: "identification_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_search_executions_search_source_search_source_id",
                        column: x => x.search_source_id,
                        principalTable: "search_source",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_search_executions_systematic_review_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_import_batches_search_execution_id",
                table: "import_batches",
                column: "search_execution_id");

            migrationBuilder.CreateIndex(
                name: "IX_identification_process_search_strategies_identification_pro~",
                table: "identification_process_search_strategies",
                columns: new[] { "identification_process_id", "search_strategy_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_identification_process_search_strategies_search_strategy_id",
                table: "identification_process_search_strategies",
                column: "search_strategy_id");

            migrationBuilder.CreateIndex(
                name: "IX_search_executions_identification_process_id",
                table: "search_executions",
                column: "identification_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_search_executions_project_id",
                table: "search_executions",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_search_executions_search_source_id",
                table: "search_executions",
                column: "search_source_id");

            migrationBuilder.AddForeignKey(
                name: "FK_import_batches_search_executions_search_execution_id",
                table: "import_batches",
                column: "search_execution_id",
                principalTable: "search_executions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
