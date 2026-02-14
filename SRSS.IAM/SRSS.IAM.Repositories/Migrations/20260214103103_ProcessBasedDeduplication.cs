using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class ProcessBasedDeduplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_papers_papers_duplicate_of_id",
                table: "papers");

            migrationBuilder.DropIndex(
                name: "IX_papers_duplicate_of_id",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "duplicate_of_id",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "is_duplicate",
                table: "papers");

            migrationBuilder.CreateTable(
                name: "deduplication_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identification_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    duplicate_of_paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    method = table.Column<string>(type: "text", nullable: false),
                    confidence_score = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deduplication_results", x => x.id);
                    table.CheckConstraint("ck_deduplication_no_self_duplicate", "paper_id != duplicate_of_paper_id");
                    table.ForeignKey(
                        name: "FK_deduplication_results_identification_processes_identificati~",
                        column: x => x.identification_process_id,
                        principalTable: "identification_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deduplication_results_papers_duplicate_of_paper_id",
                        column: x => x.duplicate_of_paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deduplication_results_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_deduplication_results_duplicate_of_paper_id",
                table: "deduplication_results",
                column: "duplicate_of_paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_deduplication_results_identification_process_id",
                table: "deduplication_results",
                column: "identification_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_deduplication_results_method",
                table: "deduplication_results",
                column: "method");

            migrationBuilder.CreateIndex(
                name: "IX_deduplication_results_paper_id",
                table: "deduplication_results",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "uq_deduplication_process_paper",
                table: "deduplication_results",
                columns: new[] { "identification_process_id", "paper_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deduplication_results");

            migrationBuilder.AddColumn<Guid>(
                name: "duplicate_of_id",
                table: "papers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_duplicate",
                table: "papers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_papers_duplicate_of_id",
                table: "papers",
                column: "duplicate_of_id");

            migrationBuilder.AddForeignKey(
                name: "FK_papers_papers_duplicate_of_id",
                table: "papers",
                column: "duplicate_of_id",
                principalTable: "papers",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
