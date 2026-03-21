using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddQAProcessIdToDecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_quality_assessment_decisions_quality_assessment_processes_Q~",
                table: "quality_assessment_decisions");

            migrationBuilder.RenameColumn(
                name: "QualityAssessmentProcessId",
                table: "quality_assessment_decisions",
                newName: "quality_assessment_process_id");

            migrationBuilder.RenameIndex(
                name: "IX_quality_assessment_decisions_QualityAssessmentProcessId",
                table: "quality_assessment_decisions",
                newName: "IX_quality_assessment_decisions_quality_assessment_process_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "quality_assessment_process_id",
                table: "quality_assessment_decisions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_quality_assessment_decisions_quality_assessment_processes_q~",
                table: "quality_assessment_decisions",
                column: "quality_assessment_process_id",
                principalTable: "quality_assessment_processes",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_quality_assessment_decisions_quality_assessment_processes_q~",
                table: "quality_assessment_decisions");

            migrationBuilder.RenameColumn(
                name: "quality_assessment_process_id",
                table: "quality_assessment_decisions",
                newName: "QualityAssessmentProcessId");

            migrationBuilder.RenameIndex(
                name: "IX_quality_assessment_decisions_quality_assessment_process_id",
                table: "quality_assessment_decisions",
                newName: "IX_quality_assessment_decisions_QualityAssessmentProcessId");

            migrationBuilder.AlterColumn<Guid>(
                name: "QualityAssessmentProcessId",
                table: "quality_assessment_decisions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_quality_assessment_decisions_quality_assessment_processes_Q~",
                table: "quality_assessment_decisions",
                column: "QualityAssessmentProcessId",
                principalTable: "quality_assessment_processes",
                principalColumn: "id");
        }
    }
}
