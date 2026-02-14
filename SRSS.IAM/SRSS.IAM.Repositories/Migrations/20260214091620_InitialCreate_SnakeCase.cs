using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate_SnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "protocol_reviewer",
                columns: table => new
                {
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    role = table.Column<string>(type: "text", nullable: true),
                    affiliation = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_protocol_reviewer", x => x.reviewer_id);
                });

            migrationBuilder.CreateTable(
                name: "question_type",
                columns: table => new
                {
                    question_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_question_type", x => x.question_type_id);
                });

            migrationBuilder.CreateTable(
                name: "search_term",
                columns: table => new
                {
                    term_id = table.Column<Guid>(type: "uuid", nullable: false),
                    keyword = table.Column<string>(type: "text", nullable: false),
                    source = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_term", x => x.term_id);
                });

            migrationBuilder.CreateTable(
                name: "systematic_review_projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    domain = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_systematic_review_projects", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    password = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    full_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    refresh_token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_refresh_token_revoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    refresh_token_expiry_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "commissioning_document",
                columns: table => new
                {
                    document_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sponsor = table.Column<string>(type: "text", nullable: true),
                    scope = table.Column<string>(type: "text", nullable: true),
                    budget = table.Column<decimal>(type: "numeric", nullable: true),
                    document_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_commissioning_document", x => x.document_id);
                    table.ForeignKey(
                        name: "FK_commissioning_document_systematic_review_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "research_question",
                columns: table => new
                {
                    research_question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_type_id = table.Column<Guid>(type: "uuid", nullable: false),
                    question_text = table.Column<string>(type: "text", nullable: false),
                    rationale = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_research_question", x => x.research_question_id);
                    table.ForeignKey(
                        name: "FK_research_question_question_type_question_type_id",
                        column: x => x.question_type_id,
                        principalTable: "question_type",
                        principalColumn: "question_type_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_research_question_systematic_review_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "review_need",
                columns: table => new
                {
                    need_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    justification = table.Column<string>(type: "text", nullable: true),
                    identified_by = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_need", x => x.need_id);
                    table.ForeignKey(
                        name: "FK_review_need_systematic_review_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "review_objective",
                columns: table => new
                {
                    objective_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    objective_statement = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_objective", x => x.objective_id);
                    table.ForeignKey(
                        name: "FK_review_objective_systematic_review_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "review_processes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    current_phase = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_review_processes", x => x.id);
                    table.ForeignKey(
                        name: "FK_review_processes_systematic_review_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "review_protocol",
                columns: table => new
                {
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocol_version = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                name: "picoc_element",
                columns: table => new
                {
                    picoc_id = table.Column<Guid>(type: "uuid", nullable: false),
                    research_question_id = table.Column<Guid>(type: "uuid", nullable: false),
                    element_type = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_picoc_element", x => x.picoc_id);
                    table.ForeignKey(
                        name: "FK_picoc_element_research_question_research_question_id",
                        column: x => x.research_question_id,
                        principalTable: "research_question",
                        principalColumn: "research_question_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "identification_processes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identification_processes", x => x.id);
                    table.ForeignKey(
                        name: "FK_identification_processes_review_processes_review_process_id",
                        column: x => x.review_process_id,
                        principalTable: "review_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_extraction_strategy",
                columns: table => new
                {
                    extraction_strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    synthesis_strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    dissemination_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    timetable_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "protocol_evaluation",
                columns: table => new
                {
                    evaluation_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    evaluation_result = table.Column<string>(type: "text", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    evaluated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ProtocolReviewerId = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                        name: "FK_protocol_evaluation_protocol_reviewer_reviewer_id",
                        column: x => x.reviewer_id,
                        principalTable: "protocol_reviewer",
                        principalColumn: "reviewer_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_protocol_evaluation_review_protocol_protocol_id",
                        column: x => x.protocol_id,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "protocol_version",
                columns: table => new
                {
                    version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    version_number = table.Column<string>(type: "text", nullable: false),
                    change_summary = table.Column<string>(type: "text", nullable: true),
                    snapshot_data = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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
                name: "quality_assessment_strategy",
                columns: table => new
                {
                    qa_strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "search_source",
                columns: table => new
                {
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_type = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_source", x => x.source_id);
                    table.ForeignKey(
                        name: "FK_search_source_review_protocol_protocol_id",
                        column: x => x.protocol_id,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "search_strategy",
                columns: table => new
                {
                    strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_strategy", x => x.strategy_id);
                    table.ForeignKey(
                        name: "FK_search_strategy_review_protocol_protocol_id",
                        column: x => x.protocol_id,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "study_selection_criteria",
                columns: table => new
                {
                    criteria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_selection_criteria", x => x.criteria_id);
                    table.ForeignKey(
                        name: "FK_study_selection_criteria_review_protocol_protocol_id",
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
                name: "comparison",
                columns: table => new
                {
                    comparison_id = table.Column<Guid>(type: "uuid", nullable: false),
                    picoc_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comparison", x => x.comparison_id);
                    table.ForeignKey(
                        name: "FK_comparison_picoc_element_picoc_id",
                        column: x => x.picoc_id,
                        principalTable: "picoc_element",
                        principalColumn: "picoc_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "context",
                columns: table => new
                {
                    context_id = table.Column<Guid>(type: "uuid", nullable: false),
                    picoc_id = table.Column<Guid>(type: "uuid", nullable: false),
                    environment = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_context", x => x.context_id);
                    table.ForeignKey(
                        name: "FK_context_picoc_element_picoc_id",
                        column: x => x.picoc_id,
                        principalTable: "picoc_element",
                        principalColumn: "picoc_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "intervention",
                columns: table => new
                {
                    intervention_id = table.Column<Guid>(type: "uuid", nullable: false),
                    picoc_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_intervention", x => x.intervention_id);
                    table.ForeignKey(
                        name: "FK_intervention_picoc_element_picoc_id",
                        column: x => x.picoc_id,
                        principalTable: "picoc_element",
                        principalColumn: "picoc_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "outcome",
                columns: table => new
                {
                    outcome_id = table.Column<Guid>(type: "uuid", nullable: false),
                    picoc_id = table.Column<Guid>(type: "uuid", nullable: false),
                    metric = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outcome", x => x.outcome_id);
                    table.ForeignKey(
                        name: "FK_outcome_picoc_element_picoc_id",
                        column: x => x.picoc_id,
                        principalTable: "picoc_element",
                        principalColumn: "picoc_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "population",
                columns: table => new
                {
                    population_id = table.Column<Guid>(type: "uuid", nullable: false),
                    picoc_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_population", x => x.population_id);
                    table.ForeignKey(
                        name: "FK_population_picoc_element_picoc_id",
                        column: x => x.picoc_id,
                        principalTable: "picoc_element",
                        principalColumn: "picoc_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "search_executions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identification_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    search_source = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    search_query = table.Column<string>(type: "text", nullable: true),
                    executed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    result_count = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_executions", x => x.id);
                    table.ForeignKey(
                        name: "FK_search_executions_identification_processes_identification_p~",
                        column: x => x.identification_process_id,
                        principalTable: "identification_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "data_extraction_form",
                columns: table => new
                {
                    form_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    checklist_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "bibliographic_database",
                columns: table => new
                {
                    database_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bibliographic_database", x => x.database_id);
                    table.ForeignKey(
                        name: "FK_bibliographic_database_search_source_source_id",
                        column: x => x.source_id,
                        principalTable: "search_source",
                        principalColumn: "source_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "conference_proceeding",
                columns: table => new
                {
                    conference_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conference_proceeding", x => x.conference_id);
                    table.ForeignKey(
                        name: "FK_conference_proceeding_search_source_source_id",
                        column: x => x.source_id,
                        principalTable: "search_source",
                        principalColumn: "source_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "digital_library",
                columns: table => new
                {
                    library_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    access_url = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_digital_library", x => x.library_id);
                    table.ForeignKey(
                        name: "FK_digital_library_search_source_source_id",
                        column: x => x.source_id,
                        principalTable: "search_source",
                        principalColumn: "source_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "journal",
                columns: table => new
                {
                    journal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_journal", x => x.journal_id);
                    table.ForeignKey(
                        name: "FK_journal_search_source_source_id",
                        column: x => x.source_id,
                        principalTable: "search_source",
                        principalColumn: "source_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "search_string",
                columns: table => new
                {
                    search_string_id = table.Column<Guid>(type: "uuid", nullable: false),
                    strategy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    expression = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_string", x => x.search_string_id);
                    table.ForeignKey(
                        name: "FK_search_string_search_strategy_strategy_id",
                        column: x => x.strategy_id,
                        principalTable: "search_strategy",
                        principalColumn: "strategy_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exclusion_criterion",
                columns: table => new
                {
                    exclusion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criteria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exclusion_criterion", x => x.exclusion_id);
                    table.ForeignKey(
                        name: "FK_exclusion_criterion_study_selection_criteria_criteria_id",
                        column: x => x.criteria_id,
                        principalTable: "study_selection_criteria",
                        principalColumn: "criteria_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "inclusion_criterion",
                columns: table => new
                {
                    inclusion_id = table.Column<Guid>(type: "uuid", nullable: false),
                    criteria_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rule = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inclusion_criterion", x => x.inclusion_id);
                    table.ForeignKey(
                        name: "FK_inclusion_criterion_study_selection_criteria_criteria_id",
                        column: x => x.criteria_id,
                        principalTable: "study_selection_criteria",
                        principalColumn: "criteria_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "import_batches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    file_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    total_records = table.Column<int>(type: "integer", nullable: false),
                    imported_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    imported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    search_execution_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_import_batches", x => x.id);
                    table.ForeignKey(
                        name: "FK_import_batches_search_executions_search_execution_id",
                        column: x => x.search_execution_id,
                        principalTable: "search_executions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "data_item_definition",
                columns: table => new
                {
                    data_item_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    quality_criterion_id = table.Column<Guid>(type: "uuid", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "search_string_term",
                columns: table => new
                {
                    search_string_id = table.Column<Guid>(type: "uuid", nullable: false),
                    term_id = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_search_string_term", x => new { x.search_string_id, x.term_id });
                    table.ForeignKey(
                        name: "FK_search_string_term_search_string_search_string_id",
                        column: x => x.search_string_id,
                        principalTable: "search_string",
                        principalColumn: "search_string_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_search_string_term_search_term_term_id",
                        column: x => x.term_id,
                        principalTable: "search_term",
                        principalColumn: "term_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "papers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    authors = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    @abstract = table.Column<string>(name: "abstract", type: "text", nullable: true),
                    doi = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    publication_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    publication_year = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    publication_year_int = table.Column<int>(type: "integer", nullable: true),
                    publication_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    volume = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    issue = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    pages = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    publisher = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    abstract_language = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    keywords = table.Column<string>(type: "text", nullable: true),
                    url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    raw_reference = table.Column<string>(type: "text", nullable: true),
                    conference_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    conference_location = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    conference_country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    conference_year = table.Column<int>(type: "integer", nullable: true),
                    conference_start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    conference_end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    journal = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    journal_issn = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    journal_e_issn = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    journal_publisher = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    source_record_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    import_batch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    imported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    imported_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    pdf_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    full_text_available = table.Column<bool>(type: "boolean", nullable: true),
                    access_type = table.Column<string>(type: "text", nullable: true),
                    current_selection_status = table.Column<string>(type: "text", nullable: false),
                    is_included_final = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    last_decision_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    internal_notes = table.Column<string>(type: "text", nullable: true),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_papers", x => x.id);
                    table.ForeignKey(
                        name: "FK_papers_import_batches_import_batch_id",
                        column: x => x.import_batch_id,
                        principalTable: "import_batches",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_papers_systematic_review_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bibliographic_database_source_id",
                table: "bibliographic_database",
                column: "source_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_commissioning_document_project_id",
                table: "commissioning_document",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_comparison_picoc_id",
                table: "comparison",
                column: "picoc_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_conference_proceeding_source_id",
                table: "conference_proceeding",
                column: "source_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_context_picoc_id",
                table: "context",
                column: "picoc_id",
                unique: true);

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
                name: "IX_digital_library_source_id",
                table: "digital_library",
                column: "source_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dissemination_strategy_protocol_id",
                table: "dissemination_strategy",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_exclusion_criterion_criteria_id",
                table: "exclusion_criterion",
                column: "criteria_id");

            migrationBuilder.CreateIndex(
                name: "IX_identification_processes_review_process_id",
                table: "identification_processes",
                column: "review_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_search_execution_id",
                table: "import_batches",
                column: "search_execution_id");

            migrationBuilder.CreateIndex(
                name: "IX_inclusion_criterion_criteria_id",
                table: "inclusion_criterion",
                column: "criteria_id");

            migrationBuilder.CreateIndex(
                name: "IX_intervention_picoc_id",
                table: "intervention",
                column: "picoc_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_journal_source_id",
                table: "journal",
                column: "source_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outcome_picoc_id",
                table: "outcome",
                column: "picoc_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_papers_doi",
                table: "papers",
                column: "doi");

            migrationBuilder.CreateIndex(
                name: "IX_papers_import_batch_id",
                table: "papers",
                column: "import_batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_papers_project_id",
                table: "papers",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_picoc_element_research_question_id",
                table: "picoc_element",
                column: "research_question_id");

            migrationBuilder.CreateIndex(
                name: "IX_population_picoc_id",
                table: "population",
                column: "picoc_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_project_timetable_protocol_id",
                table: "project_timetable",
                column: "protocol_id");

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
                name: "IX_research_question_project_id",
                table: "research_question",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_research_question_question_type_id",
                table: "research_question",
                column: "question_type_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_need_project_id",
                table: "review_need",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_review_objective_project_id",
                table: "review_objective",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_review_process_project_id",
                table: "review_processes",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "idx_review_process_status",
                table: "review_processes",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_review_protocol_project_id",
                table: "review_protocol",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_search_executions_identification_process_id",
                table: "search_executions",
                column: "identification_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_search_source_protocol_id",
                table: "search_source",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_search_strategy_protocol_id",
                table: "search_strategy",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_search_string_strategy_id",
                table: "search_string",
                column: "strategy_id");

            migrationBuilder.CreateIndex(
                name: "IX_search_string_term_term_id",
                table: "search_string_term",
                column: "term_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_criteria_protocol_id",
                table: "study_selection_criteria",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_procedure_protocol_id",
                table: "study_selection_procedure",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "idx_project_status",
                table: "systematic_review_projects",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_project_title",
                table: "systematic_review_projects",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_is_active",
                table: "users",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bibliographic_database");

            migrationBuilder.DropTable(
                name: "commissioning_document");

            migrationBuilder.DropTable(
                name: "comparison");

            migrationBuilder.DropTable(
                name: "conference_proceeding");

            migrationBuilder.DropTable(
                name: "context");

            migrationBuilder.DropTable(
                name: "data_item_definition");

            migrationBuilder.DropTable(
                name: "data_synthesis_strategy");

            migrationBuilder.DropTable(
                name: "digital_library");

            migrationBuilder.DropTable(
                name: "dissemination_strategy");

            migrationBuilder.DropTable(
                name: "exclusion_criterion");

            migrationBuilder.DropTable(
                name: "inclusion_criterion");

            migrationBuilder.DropTable(
                name: "intervention");

            migrationBuilder.DropTable(
                name: "journal");

            migrationBuilder.DropTable(
                name: "outcome");

            migrationBuilder.DropTable(
                name: "papers");

            migrationBuilder.DropTable(
                name: "population");

            migrationBuilder.DropTable(
                name: "project_timetable");

            migrationBuilder.DropTable(
                name: "protocol_evaluation");

            migrationBuilder.DropTable(
                name: "protocol_version");

            migrationBuilder.DropTable(
                name: "quality_criterion");

            migrationBuilder.DropTable(
                name: "review_need");

            migrationBuilder.DropTable(
                name: "review_objective");

            migrationBuilder.DropTable(
                name: "search_string_term");

            migrationBuilder.DropTable(
                name: "study_selection_procedure");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "data_extraction_form");

            migrationBuilder.DropTable(
                name: "study_selection_criteria");

            migrationBuilder.DropTable(
                name: "search_source");

            migrationBuilder.DropTable(
                name: "import_batches");

            migrationBuilder.DropTable(
                name: "picoc_element");

            migrationBuilder.DropTable(
                name: "protocol_reviewer");

            migrationBuilder.DropTable(
                name: "quality_checklist");

            migrationBuilder.DropTable(
                name: "search_string");

            migrationBuilder.DropTable(
                name: "search_term");

            migrationBuilder.DropTable(
                name: "data_extraction_strategy");

            migrationBuilder.DropTable(
                name: "search_executions");

            migrationBuilder.DropTable(
                name: "research_question");

            migrationBuilder.DropTable(
                name: "quality_assessment_strategy");

            migrationBuilder.DropTable(
                name: "search_strategy");

            migrationBuilder.DropTable(
                name: "identification_processes");

            migrationBuilder.DropTable(
                name: "question_type");

            migrationBuilder.DropTable(
                name: "review_protocol");

            migrationBuilder.DropTable(
                name: "review_processes");

            migrationBuilder.DropTable(
                name: "systematic_review_projects");
        }
    }
}
