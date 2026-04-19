using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class submissioncontext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_study_selection_checklist_submissions_papers_PaperId",
                table: "study_selection_checklist_submissions");

            migrationBuilder.DropForeignKey(
                name: "FK_study_selection_checklist_submissions_screening_decisions_s~",
                table: "study_selection_checklist_submissions");

            migrationBuilder.DropTable(
                name: "study_characteristics");

            migrationBuilder.DropIndex(
                name: "IX_study_selection_checklist_submissions_screening_decision_id",
                table: "study_selection_checklist_submissions");

            migrationBuilder.RenameColumn(
                name: "PaperId",
                table: "study_selection_checklist_submissions",
                newName: "paper_id");

            migrationBuilder.RenameColumn(
                name: "screening_decision_id",
                table: "study_selection_checklist_submissions",
                newName: "study_selection_process_id");

            migrationBuilder.RenameIndex(
                name: "IX_study_selection_checklist_submissions_PaperId",
                table: "study_selection_checklist_submissions",
                newName: "IX_study_selection_checklist_submissions_paper_id");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "submitted_at",
                table: "study_selection_checklist_submissions",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "paper_id",
                table: "study_selection_checklist_submissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "reviewer_id",
                table: "study_selection_checklist_submissions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "screening_phase",
                table: "study_selection_checklist_submissions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "checklist_submission_id",
                table: "screening_decisions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_checklist_submissions_reviewer_id",
                table: "study_selection_checklist_submissions",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_checklist_submissions_study_selection_proce~",
                table: "study_selection_checklist_submissions",
                column: "study_selection_process_id");

            migrationBuilder.CreateIndex(
                name: "uq_study_selection_checklist_submission_context",
                table: "study_selection_checklist_submissions",
                columns: new[] { "study_selection_process_id", "paper_id", "reviewer_id", "screening_phase" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_screening_decisions_checklist_submission_id",
                table: "screening_decisions",
                column: "checklist_submission_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_screening_decisions_study_selection_checklist_submissions_c~",
                table: "screening_decisions",
                column: "checklist_submission_id",
                principalTable: "study_selection_checklist_submissions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_study_selection_checklist_submissions_papers_paper_id",
                table: "study_selection_checklist_submissions",
                column: "paper_id",
                principalTable: "papers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_screening_decisions_study_selection_checklist_submissions_c~",
                table: "screening_decisions");

            migrationBuilder.DropForeignKey(
                name: "FK_study_selection_checklist_submissions_papers_paper_id",
                table: "study_selection_checklist_submissions");

            migrationBuilder.DropIndex(
                name: "IX_study_selection_checklist_submissions_reviewer_id",
                table: "study_selection_checklist_submissions");

            migrationBuilder.DropIndex(
                name: "IX_study_selection_checklist_submissions_study_selection_proce~",
                table: "study_selection_checklist_submissions");

            migrationBuilder.DropIndex(
                name: "uq_study_selection_checklist_submission_context",
                table: "study_selection_checklist_submissions");

            migrationBuilder.DropIndex(
                name: "IX_screening_decisions_checklist_submission_id",
                table: "screening_decisions");

            migrationBuilder.DropColumn(
                name: "reviewer_id",
                table: "study_selection_checklist_submissions");

            migrationBuilder.DropColumn(
                name: "screening_phase",
                table: "study_selection_checklist_submissions");

            migrationBuilder.DropColumn(
                name: "checklist_submission_id",
                table: "screening_decisions");

            migrationBuilder.RenameColumn(
                name: "paper_id",
                table: "study_selection_checklist_submissions",
                newName: "PaperId");

            migrationBuilder.RenameColumn(
                name: "study_selection_process_id",
                table: "study_selection_checklist_submissions",
                newName: "screening_decision_id");

            migrationBuilder.RenameIndex(
                name: "IX_study_selection_checklist_submissions_paper_id",
                table: "study_selection_checklist_submissions",
                newName: "IX_study_selection_checklist_submissions_PaperId");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "submitted_at",
                table: "study_selection_checklist_submissions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "PaperId",
                table: "study_selection_checklist_submissions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "study_characteristics",
                columns: table => new
                {
                    study_characteristic_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    domain = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    language = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    study_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_characteristics", x => x.study_characteristic_id);
                    table.ForeignKey(
                        name: "FK_study_characteristics_review_protocol_protocol_id",
                        column: x => x.protocol_id,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_checklist_submissions_screening_decision_id",
                table: "study_selection_checklist_submissions",
                column: "screening_decision_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_study_characteristics_protocol_id",
                table: "study_characteristics",
                column: "protocol_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_study_selection_checklist_submissions_papers_PaperId",
                table: "study_selection_checklist_submissions",
                column: "PaperId",
                principalTable: "papers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_study_selection_checklist_submissions_screening_decisions_s~",
                table: "study_selection_checklist_submissions",
                column: "screening_decision_id",
                principalTable: "screening_decisions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
