using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class PrismaReporting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "prisma_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    generated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    generated_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prisma_reports", x => x.id);
                    table.ForeignKey(
                        name: "FK_prisma_reports_systematic_review_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prisma_flow_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    prisma_report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stage = table.Column<string>(type: "text", nullable: false),
                    label = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    count = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prisma_flow_records", x => x.id);
                    table.ForeignKey(
                        name: "FK_prisma_flow_records_prisma_reports_prisma_report_id",
                        column: x => x.prisma_report_id,
                        principalTable: "prisma_reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_prisma_flow_records_prisma_report_id",
                table: "prisma_flow_records",
                column: "prisma_report_id");

            migrationBuilder.CreateIndex(
                name: "IX_prisma_flow_records_stage",
                table: "prisma_flow_records",
                column: "stage");

            migrationBuilder.CreateIndex(
                name: "IX_prisma_reports_generated_at",
                table: "prisma_reports",
                column: "generated_at");

            migrationBuilder.CreateIndex(
                name: "IX_prisma_reports_project_id",
                table: "prisma_reports",
                column: "project_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prisma_flow_records");

            migrationBuilder.DropTable(
                name: "prisma_reports");
        }
    }
}
