using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class upd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_papers_is_removed_as_duplicate",
                table: "papers");

            migrationBuilder.DropColumn(
                name: "is_removed_as_duplicate",
                table: "papers");

            migrationBuilder.CreateTable(
                name: "identification_process_papers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identification_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    included_after_dedup = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identification_process_papers", x => x.id);
                    table.ForeignKey(
                        name: "FK_identification_process_papers_identification_processes_iden~",
                        column: x => x.identification_process_id,
                        principalTable: "identification_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_identification_process_papers_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_identification_process_papers_identification_process_id",
                table: "identification_process_papers",
                column: "identification_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_identification_process_papers_included_after_dedup",
                table: "identification_process_papers",
                column: "included_after_dedup");

            migrationBuilder.CreateIndex(
                name: "IX_identification_process_papers_paper_id",
                table: "identification_process_papers",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "uq_identification_process_paper",
                table: "identification_process_papers",
                columns: new[] { "identification_process_id", "paper_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "identification_process_papers");

            migrationBuilder.AddColumn<bool>(
                name: "is_removed_as_duplicate",
                table: "papers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_papers_is_removed_as_duplicate",
                table: "papers",
                column: "is_removed_as_duplicate");
        }
    }
}
