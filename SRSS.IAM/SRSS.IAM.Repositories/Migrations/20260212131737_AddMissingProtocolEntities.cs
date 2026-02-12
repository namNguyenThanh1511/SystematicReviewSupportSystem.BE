using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingProtocolEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "data_extraction_strategy",
                columns: table => new
                {
                    extraction_strategy_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_extraction_strategy", x => x.extraction_strategy_id);
                    table.ForeignKey(
                        name: "FK_data_extraction_strategy_review_protocol_protocol_id",
                        column: x => x.protocol_id,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_synthesis_strategy",
                columns: table => new
                {
                    synthesis_strategy_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    synthesis_type = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_synthesis_strategy", x => x.synthesis_strategy_id);
                    table.ForeignKey(
                        name: "FK_data_synthesis_strategy_review_protocol_protocol_id",
                        column: x => x.protocol_id,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "dissemination_strategy",
                columns: table => new
                {
                    dissemination_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    channel = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dissemination_strategy", x => x.dissemination_id);
                    table.ForeignKey(
                        name: "FK_dissemination_strategy_review_protocol_protocol_id",
                        column: x => x.protocol_id,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_timetable",
                columns: table => new
                {
                    timetable_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    milestone = table.Column<string>(type: "text", nullable: false),
                    planned_date = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_timetable", x => x.timetable_id);
                    table.ForeignKey(
                        name: "FK_project_timetable_review_protocol_protocol_id",
                        column: x => x.protocol_id,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quality_assessment_strategy",
                columns: table => new
                {
                    qa_strategy_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quality_assessment_strategy", x => x.qa_strategy_id);
                    table.ForeignKey(
                        name: "FK_quality_assessment_strategy_review_protocol_protocol_id",
                        column: x => x.protocol_id,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "study_selection_procedure",
                columns: table => new
                {
                    procedure_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    steps = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "data_extraction_form",
                columns: table => new
                {
                    form_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    extraction_strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_extraction_form", x => x.form_id);
                    table.ForeignKey(
                        name: "FK_data_extraction_form_data_extraction_strategy_extraction_st~",
                        column: x => x.extraction_strategy_id,
                        principalTable: "data_extraction_strategy",
                        principalColumn: "extraction_strategy_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quality_checklist",
                columns: table => new
                {
                    checklist_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    qa_strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quality_checklist", x => x.checklist_id);
                    table.ForeignKey(
                        name: "FK_quality_checklist_quality_assessment_strategy_qa_strategy_id",
                        column: x => x.qa_strategy_id,
                        principalTable: "quality_assessment_strategy",
                        principalColumn: "qa_strategy_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_item_definition",
                columns: table => new
                {
                    data_item_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    form_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    data_type = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_item_definition", x => x.data_item_id);
                    table.ForeignKey(
                        name: "FK_data_item_definition_data_extraction_form_form_id",
                        column: x => x.form_id,
                        principalTable: "data_extraction_form",
                        principalColumn: "form_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quality_criterion",
                columns: table => new
                {
                    quality_criterion_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    checklist_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question = table.Column<string>(type: "text", nullable: false),
                    weight = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_quality_criterion", x => x.quality_criterion_id);
                    table.ForeignKey(
                        name: "FK_quality_criterion_quality_checklist_checklist_id",
                        column: x => x.checklist_id,
                        principalTable: "quality_checklist",
                        principalColumn: "checklist_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_data_extraction_form_extraction_strategy_id",
                table: "data_extraction_form",
                column: "extraction_strategy_id");

            migrationBuilder.CreateIndex(
                name: "IX_data_extraction_strategy_protocol_id",
                table: "data_extraction_strategy",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_data_item_definition_form_id",
                table: "data_item_definition",
                column: "form_id");

            migrationBuilder.CreateIndex(
                name: "IX_data_synthesis_strategy_protocol_id",
                table: "data_synthesis_strategy",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_dissemination_strategy_protocol_id",
                table: "dissemination_strategy",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_timetable_protocol_id",
                table: "project_timetable",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_assessment_strategy_protocol_id",
                table: "quality_assessment_strategy",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_checklist_qa_strategy_id",
                table: "quality_checklist",
                column: "qa_strategy_id");

            migrationBuilder.CreateIndex(
                name: "IX_quality_criterion_checklist_id",
                table: "quality_criterion",
                column: "checklist_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_procedure_protocol_id",
                table: "study_selection_procedure",
                column: "protocol_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "data_item_definition");

            migrationBuilder.DropTable(
                name: "data_synthesis_strategy");

            migrationBuilder.DropTable(
                name: "dissemination_strategy");

            migrationBuilder.DropTable(
                name: "project_timetable");

            migrationBuilder.DropTable(
                name: "quality_criterion");

            migrationBuilder.DropTable(
                name: "study_selection_procedure");

            migrationBuilder.DropTable(
                name: "data_extraction_form");

            migrationBuilder.DropTable(
                name: "quality_checklist");

            migrationBuilder.DropTable(
                name: "data_extraction_strategy");

            migrationBuilder.DropTable(
                name: "quality_assessment_strategy");
        }
    }
}
