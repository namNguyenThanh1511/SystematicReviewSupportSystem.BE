using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProtocolModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_data_synthesis_strategy_review_protocol_protocol_id",
                table: "data_synthesis_strategy");

            migrationBuilder.DropForeignKey(
                name: "FK_extraction_template_review_protocol_ProtocolId",
                table: "extraction_template");

            migrationBuilder.DropForeignKey(
                name: "FK_quality_assessment_strategy_review_protocol_protocol_id",
                table: "quality_assessment_strategy");

            migrationBuilder.DropForeignKey(
                name: "FK_review_processes_review_protocol_protocol_id",
                table: "review_processes");

            migrationBuilder.DropForeignKey(
                name: "FK_search_source_review_protocol_protocol_id",
                table: "search_source");

            migrationBuilder.DropForeignKey(
                name: "FK_study_selection_criteria_review_protocol_protocol_id",
                table: "study_selection_criteria");

            migrationBuilder.DropTable(
                name: "protocol_evaluation");

            migrationBuilder.DropTable(
                name: "protocol_version");

            migrationBuilder.DropTable(
                name: "study_selection_procedure");

            migrationBuilder.DropTable(
                name: "protocol_reviewer");

            migrationBuilder.DropTable(
                name: "review_protocol");

            migrationBuilder.DropIndex(
                name: "IX_review_processes_protocol_id",
                table: "review_processes");

            migrationBuilder.DropColumn(
                name: "protocol_id",
                table: "review_processes");

            migrationBuilder.RenameColumn(
                name: "protocol_id",
                table: "study_selection_criteria",
                newName: "project_id");

            migrationBuilder.RenameIndex(
                name: "IX_study_selection_criteria_protocol_id",
                table: "study_selection_criteria",
                newName: "IX_study_selection_criteria_project_id");

            migrationBuilder.RenameColumn(
                name: "protocol_id",
                table: "search_source",
                newName: "project_id");

            migrationBuilder.RenameIndex(
                name: "IX_search_source_protocol_id",
                table: "search_source",
                newName: "IX_search_source_project_id");

            migrationBuilder.RenameColumn(
                name: "protocol_id",
                table: "quality_assessment_strategy",
                newName: "project_id");

            migrationBuilder.RenameIndex(
                name: "IX_quality_assessment_strategy_protocol_id",
                table: "quality_assessment_strategy",
                newName: "IX_quality_assessment_strategy_project_id");

            migrationBuilder.RenameColumn(
                name: "ProtocolId",
                table: "extraction_template",
                newName: "ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_extraction_template_ProtocolId",
                table: "extraction_template",
                newName: "IX_extraction_template_ProjectId");

            migrationBuilder.RenameColumn(
                name: "protocol_id",
                table: "data_synthesis_strategy",
                newName: "project_id");

            migrationBuilder.RenameIndex(
                name: "IX_data_synthesis_strategy_protocol_id",
                table: "data_synthesis_strategy",
                newName: "IX_data_synthesis_strategy_project_id");

            // Clear problematic tables to avoid FK violations with old protocol IDs
            migrationBuilder.Sql("DELETE FROM study_selection_criteria");
            migrationBuilder.Sql("DELETE FROM search_source");
            migrationBuilder.Sql("DELETE FROM quality_assessment_strategy");
            migrationBuilder.Sql("DELETE FROM extraction_template");
            migrationBuilder.Sql("DELETE FROM data_synthesis_strategy");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "study_selection_criteria",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "quality_assessment_strategy",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_data_synthesis_strategy_systematic_review_projects_project_~",
                table: "data_synthesis_strategy",
                column: "project_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_extraction_template_systematic_review_projects_ProjectId",
                table: "extraction_template",
                column: "ProjectId",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_quality_assessment_strategy_systematic_review_projects_proj~",
                table: "quality_assessment_strategy",
                column: "project_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_search_source_systematic_review_projects_project_id",
                table: "search_source",
                column: "project_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_study_selection_criteria_systematic_review_projects_project~",
                table: "study_selection_criteria",
                column: "project_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_data_synthesis_strategy_systematic_review_projects_project_~",
                table: "data_synthesis_strategy");

            migrationBuilder.DropForeignKey(
                name: "FK_extraction_template_systematic_review_projects_ProjectId",
                table: "extraction_template");

            migrationBuilder.DropForeignKey(
                name: "FK_quality_assessment_strategy_systematic_review_projects_proj~",
                table: "quality_assessment_strategy");

            migrationBuilder.DropForeignKey(
                name: "FK_search_source_systematic_review_projects_project_id",
                table: "search_source");

            migrationBuilder.DropForeignKey(
                name: "FK_study_selection_criteria_systematic_review_projects_project~",
                table: "study_selection_criteria");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "study_selection_criteria",
                newName: "protocol_id");

            migrationBuilder.RenameIndex(
                name: "IX_study_selection_criteria_project_id",
                table: "study_selection_criteria",
                newName: "IX_study_selection_criteria_protocol_id");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "search_source",
                newName: "protocol_id");

            migrationBuilder.RenameIndex(
                name: "IX_search_source_project_id",
                table: "search_source",
                newName: "IX_search_source_protocol_id");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "quality_assessment_strategy",
                newName: "protocol_id");

            migrationBuilder.RenameIndex(
                name: "IX_quality_assessment_strategy_project_id",
                table: "quality_assessment_strategy",
                newName: "IX_quality_assessment_strategy_protocol_id");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "extraction_template",
                newName: "ProtocolId");

            migrationBuilder.RenameIndex(
                name: "IX_extraction_template_ProjectId",
                table: "extraction_template",
                newName: "IX_extraction_template_ProtocolId");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "data_synthesis_strategy",
                newName: "protocol_id");

            migrationBuilder.RenameIndex(
                name: "IX_data_synthesis_strategy_project_id",
                table: "data_synthesis_strategy",
                newName: "IX_data_synthesis_strategy_protocol_id");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "study_selection_criteria",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "protocol_id",
                table: "review_processes",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "quality_assessment_strategy",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "protocol_reviewer",
                columns: table => new
                {
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    affiliation = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_protocol_reviewer", x => x.reviewer_id);
                });

            migrationBuilder.CreateTable(
                name: "review_protocol",
                columns: table => new
                {
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    protocol_version = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_protocol", x => x.protocol_id);
                    table.ForeignKey(
                        name: "FK_review_protocol_systematic_review_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "protocol_evaluation",
                columns: table => new
                {
                    evaluation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    evaluated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    evaluation_result = table.Column<string>(type: "text", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProtocolReviewerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_protocol_evaluation", x => x.evaluation_id);
                    table.ForeignKey(
                        name: "FK_protocol_evaluation_protocol_reviewer_ProtocolReviewerId",
                        column: x => x.ProtocolReviewerId,
                        principalTable: "protocol_reviewer",
                        principalColumn: "reviewer_id");
                    table.ForeignKey(
                        name: "FK_protocol_evaluation_review_protocol_protocol_id",
                        column: x => x.protocol_id,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_protocol_evaluation_users_reviewer_id",
                        column: x => x.reviewer_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "protocol_version",
                columns: table => new
                {
                    version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    change_summary = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    snapshot_data = table.Column<string>(type: "text", nullable: false),
                    version_number = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_protocol_version", x => x.version_id);
                    table.ForeignKey(
                        name: "FK_protocol_version_review_protocol_protocol_id",
                        column: x => x.protocol_id,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "study_selection_procedure",
                columns: table => new
                {
                    procedure_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    steps = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_selection_procedure", x => x.procedure_id);
                    table.ForeignKey(
                        name: "FK_study_selection_procedure_review_protocol_protocol_id",
                        column: x => x.protocol_id,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_review_processes_protocol_id",
                table: "review_processes",
                column: "protocol_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_protocol_evaluation_protocol_id",
                table: "protocol_evaluation",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_protocol_evaluation_ProtocolReviewerId",
                table: "protocol_evaluation",
                column: "ProtocolReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_protocol_evaluation_reviewer_id",
                table: "protocol_evaluation",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "IX_protocol_version_protocol_id",
                table: "protocol_version",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_protocol_project_id",
                table: "review_protocol",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_procedure_protocol_id",
                table: "study_selection_procedure",
                column: "protocol_id");

            migrationBuilder.AddForeignKey(
                name: "FK_data_synthesis_strategy_review_protocol_protocol_id",
                table: "data_synthesis_strategy",
                column: "protocol_id",
                principalTable: "review_protocol",
                principalColumn: "protocol_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_extraction_template_review_protocol_ProtocolId",
                table: "extraction_template",
                column: "ProtocolId",
                principalTable: "review_protocol",
                principalColumn: "protocol_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_quality_assessment_strategy_review_protocol_protocol_id",
                table: "quality_assessment_strategy",
                column: "protocol_id",
                principalTable: "review_protocol",
                principalColumn: "protocol_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_review_processes_review_protocol_protocol_id",
                table: "review_processes",
                column: "protocol_id",
                principalTable: "review_protocol",
                principalColumn: "protocol_id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_search_source_review_protocol_protocol_id",
                table: "search_source",
                column: "protocol_id",
                principalTable: "review_protocol",
                principalColumn: "protocol_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_study_selection_criteria_review_protocol_protocol_id",
                table: "study_selection_criteria",
                column: "protocol_id",
                principalTable: "review_protocol",
                principalColumn: "protocol_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
