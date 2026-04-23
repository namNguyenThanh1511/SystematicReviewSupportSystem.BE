using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class UseIncludedPaperInQA : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_quality_assessment_assignment_papers_QualityAssessmentPaper~",
                table: "quality_assessment_assignment_papers");

            migrationBuilder.DropForeignKey(
                name: "FK_quality_assessment_assignments_papers_PaperId",
                table: "quality_assessment_assignments");

            migrationBuilder.DropForeignKey(
                name: "FK_quality_assessment_decisions_QualityAssessmentPapers_qualit~",
                table: "quality_assessment_decisions");

            migrationBuilder.DropForeignKey(
                name: "FK_quality_assessment_decisions_papers_PaperId",
                table: "quality_assessment_decisions");

            migrationBuilder.DropForeignKey(
                name: "FK_quality_assessment_resolutions_QualityAssessmentPapers_qual~",
                table: "quality_assessment_resolutions");

            migrationBuilder.DropTable(
                name: "QualityAssessmentPapers");

            migrationBuilder.DropIndex(
                name: "IX_quality_assessment_decisions_quality_assessment_paper_id",
                table: "quality_assessment_decisions");

            migrationBuilder.DropIndex(
                name: "IX_quality_assessment_assignments_PaperId",
                table: "quality_assessment_assignments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_quality_assessment_assignment_papers",
                table: "quality_assessment_assignment_papers");

            migrationBuilder.DropIndex(
                name: "IX_quality_assessment_assignment_papers_quality_assessment_pap~",
                table: "quality_assessment_assignment_papers");

            migrationBuilder.DropColumn(
                name: "quality_assessment_paper_id",
                table: "quality_assessment_decisions");

            migrationBuilder.DropColumn(
                name: "PaperId",
                table: "quality_assessment_assignments");

            migrationBuilder.RenameColumn(
                name: "quality_assessment_paper_id",
                table: "quality_assessment_resolutions",
                newName: "paper_id");

            migrationBuilder.RenameIndex(
                name: "IX_quality_assessment_resolutions_quality_assessment_paper_id",
                table: "quality_assessment_resolutions",
                newName: "IX_quality_assessment_resolutions_paper_id");

            migrationBuilder.RenameColumn(
                name: "PaperId",
                table: "quality_assessment_decisions",
                newName: "paper_id");

            migrationBuilder.RenameIndex(
                name: "IX_quality_assessment_decisions_PaperId",
                table: "quality_assessment_decisions",
                newName: "IX_quality_assessment_decisions_paper_id");

            migrationBuilder.RenameColumn(
                name: "quality_assessment_paper_id",
                table: "quality_assessment_assignment_papers",
                newName: "paper_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "paper_id",
                table: "quality_assessment_decisions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_quality_assessment_assignment_papers",
                table: "quality_assessment_assignment_papers",
                columns: new[] { "paper_id", "quality_assessment_assignment_id" });

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_assignment_papers_quality_assessment_ass~",
                table: "quality_assessment_assignment_papers",
                column: "quality_assessment_assignment_id");

            migrationBuilder.AddForeignKey(
                name: "FK_quality_assessment_assignment_papers_papers_paper_id",
                table: "quality_assessment_assignment_papers",
                column: "paper_id",
                principalTable: "papers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_quality_assessment_decisions_papers_paper_id",
                table: "quality_assessment_decisions",
                column: "paper_id",
                principalTable: "papers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_quality_assessment_resolutions_papers_paper_id",
                table: "quality_assessment_resolutions",
                column: "paper_id",
                principalTable: "papers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_quality_assessment_assignment_papers_papers_paper_id",
                table: "quality_assessment_assignment_papers");

            migrationBuilder.DropForeignKey(
                name: "FK_quality_assessment_decisions_papers_paper_id",
                table: "quality_assessment_decisions");

            migrationBuilder.DropForeignKey(
                name: "FK_quality_assessment_resolutions_papers_paper_id",
                table: "quality_assessment_resolutions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_quality_assessment_assignment_papers",
                table: "quality_assessment_assignment_papers");

            migrationBuilder.DropIndex(
                name: "IX_quality_assessment_assignment_papers_quality_assessment_ass~",
                table: "quality_assessment_assignment_papers");

            migrationBuilder.RenameColumn(
                name: "paper_id",
                table: "quality_assessment_resolutions",
                newName: "quality_assessment_paper_id");

            migrationBuilder.RenameIndex(
                name: "IX_quality_assessment_resolutions_paper_id",
                table: "quality_assessment_resolutions",
                newName: "IX_quality_assessment_resolutions_quality_assessment_paper_id");

            migrationBuilder.RenameColumn(
                name: "paper_id",
                table: "quality_assessment_decisions",
                newName: "PaperId");

            migrationBuilder.RenameIndex(
                name: "IX_quality_assessment_decisions_paper_id",
                table: "quality_assessment_decisions",
                newName: "IX_quality_assessment_decisions_PaperId");

            migrationBuilder.RenameColumn(
                name: "paper_id",
                table: "quality_assessment_assignment_papers",
                newName: "quality_assessment_paper_id");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaperId",
                table: "quality_assessment_decisions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "quality_assessment_paper_id",
                table: "quality_assessment_decisions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "PaperId",
                table: "quality_assessment_assignments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_quality_assessment_assignment_papers",
                table: "quality_assessment_assignment_papers",
                columns: new[] { "quality_assessment_assignment_id", "quality_assessment_paper_id" });

            migrationBuilder.CreateTable(
                name: "QualityAssessmentPapers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaperId = table.Column<Guid>(type: "uuid", nullable: false),
                    QualityAssessmentProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityAssessmentPapers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityAssessmentPapers_papers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QualityAssessmentPapers_quality_assessment_processes_Qualit~",
                        column: x => x.QualityAssessmentProcessId,
                        principalTable: "quality_assessment_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_decisions_quality_assessment_paper_id",
                table: "quality_assessment_decisions",
                column: "quality_assessment_paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_assignments_PaperId",
                table: "quality_assessment_assignments",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_assignment_papers_quality_assessment_pap~",
                table: "quality_assessment_assignment_papers",
                column: "quality_assessment_paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_QualityAssessmentPapers_PaperId",
                table: "QualityAssessmentPapers",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityAssessmentPapers_QualityAssessmentProcessId_PaperId",
                table: "QualityAssessmentPapers",
                columns: new[] { "QualityAssessmentProcessId", "PaperId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_quality_assessment_assignment_papers_QualityAssessmentPaper~",
                table: "quality_assessment_assignment_papers",
                column: "quality_assessment_paper_id",
                principalTable: "QualityAssessmentPapers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_quality_assessment_assignments_papers_PaperId",
                table: "quality_assessment_assignments",
                column: "PaperId",
                principalTable: "papers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_quality_assessment_decisions_QualityAssessmentPapers_qualit~",
                table: "quality_assessment_decisions",
                column: "quality_assessment_paper_id",
                principalTable: "QualityAssessmentPapers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_quality_assessment_decisions_papers_PaperId",
                table: "quality_assessment_decisions",
                column: "PaperId",
                principalTable: "papers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_quality_assessment_resolutions_QualityAssessmentPapers_qual~",
                table: "quality_assessment_resolutions",
                column: "quality_assessment_paper_id",
                principalTable: "QualityAssessmentPapers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
