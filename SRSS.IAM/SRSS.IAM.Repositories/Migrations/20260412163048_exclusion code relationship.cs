using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class exclusioncoderelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExclusionReasonCode",
                table: "screening_resolutions");

            migrationBuilder.DropColumn(
                name: "exclusion_reason_code",
                table: "screening_decisions");

            migrationBuilder.AddColumn<Guid>(
                name: "exclusion_reason_id",
                table: "screening_resolutions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "exclusion_reason_id",
                table: "screening_decisions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_screening_resolutions_exclusion_reason_id",
                table: "screening_resolutions",
                column: "exclusion_reason_id");

            migrationBuilder.CreateIndex(
                name: "IX_screening_decisions_exclusion_reason_id",
                table: "screening_decisions",
                column: "exclusion_reason_id");

            migrationBuilder.AddForeignKey(
                name: "FK_screening_decisions_study_selection_exclusion_reasons_exclu~",
                table: "screening_decisions",
                column: "exclusion_reason_id",
                principalTable: "study_selection_exclusion_reasons",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_screening_resolutions_study_selection_exclusion_reasons_exc~",
                table: "screening_resolutions",
                column: "exclusion_reason_id",
                principalTable: "study_selection_exclusion_reasons",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_screening_decisions_study_selection_exclusion_reasons_exclu~",
                table: "screening_decisions");

            migrationBuilder.DropForeignKey(
                name: "FK_screening_resolutions_study_selection_exclusion_reasons_exc~",
                table: "screening_resolutions");

            migrationBuilder.DropIndex(
                name: "IX_screening_resolutions_exclusion_reason_id",
                table: "screening_resolutions");

            migrationBuilder.DropIndex(
                name: "IX_screening_decisions_exclusion_reason_id",
                table: "screening_decisions");

            migrationBuilder.DropColumn(
                name: "exclusion_reason_id",
                table: "screening_resolutions");

            migrationBuilder.DropColumn(
                name: "exclusion_reason_id",
                table: "screening_decisions");

            migrationBuilder.AddColumn<int>(
                name: "ExclusionReasonCode",
                table: "screening_resolutions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "exclusion_reason_code",
                table: "screening_decisions",
                type: "text",
                nullable: true);
        }
    }
}
