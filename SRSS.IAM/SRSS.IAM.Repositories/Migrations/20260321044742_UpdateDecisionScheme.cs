using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDecisionScheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_quality_assessment_decisions_quality_criterion_quality_crit~",
                table: "quality_assessment_decisions");

            migrationBuilder.DropIndex(
                name: "IX_quality_assessment_decisions_quality_criterion_id",
                table: "quality_assessment_decisions");

            migrationBuilder.DropColumn(
                name: "comment",
                table: "quality_assessment_decisions");

            migrationBuilder.DropColumn(
                name: "quality_criterion_id",
                table: "quality_assessment_decisions");

            migrationBuilder.DropColumn(
                name: "value",
                table: "quality_assessment_decisions");

            migrationBuilder.AddColumn<decimal>(
                name: "score",
                table: "quality_assessment_decisions",
                type: "numeric(5,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "quality_assessment_decision_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    quality_assessment_decision_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quality_criterion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<int>(type: "integer", nullable: true),
                    comment = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quality_assessment_decision_items", x => x.id);
                    table.ForeignKey(
                        name: "FK_quality_assessment_decision_items_quality_assessment_decisi~",
                        column: x => x.quality_assessment_decision_id,
                        principalTable: "quality_assessment_decisions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_quality_assessment_decision_items_quality_criterion_quality~",
                        column: x => x.quality_criterion_id,
                        principalTable: "quality_criterion",
                        principalColumn: "quality_criterion_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_decision_items_quality_assessment_decisi~",
                table: "quality_assessment_decision_items",
                column: "quality_assessment_decision_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_decision_items_quality_criterion_id",
                table: "quality_assessment_decision_items",
                column: "quality_criterion_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "quality_assessment_decision_items");

            migrationBuilder.DropColumn(
                name: "score",
                table: "quality_assessment_decisions");

            migrationBuilder.AddColumn<string>(
                name: "comment",
                table: "quality_assessment_decisions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "quality_criterion_id",
                table: "quality_assessment_decisions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "value",
                table: "quality_assessment_decisions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_decisions_quality_criterion_id",
                table: "quality_assessment_decisions",
                column: "quality_criterion_id");

            migrationBuilder.AddForeignKey(
                name: "FK_quality_assessment_decisions_quality_criterion_quality_crit~",
                table: "quality_assessment_decisions",
                column: "quality_criterion_id",
                principalTable: "quality_criterion",
                principalColumn: "quality_criterion_id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
