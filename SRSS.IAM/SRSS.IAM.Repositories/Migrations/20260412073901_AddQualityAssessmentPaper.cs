using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddQualityAssessmentPaper : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QualityAssessmentPapers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    QualityAssessmentProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaperId = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "IX_QualityAssessmentPapers_PaperId",
                table: "QualityAssessmentPapers",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityAssessmentPapers_QualityAssessmentProcessId_PaperId",
                table: "QualityAssessmentPapers",
                columns: new[] { "QualityAssessmentProcessId", "PaperId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QualityAssessmentPapers");
        }
    }
}
