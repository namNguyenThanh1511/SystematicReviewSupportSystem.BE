using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProtocolReviewers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: true),
                    Affiliation = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtocolReviewers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SearchTerms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Keyword = table.Column<string>(type: "text", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchTerms", x => x.Id);
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
                name: "CommissioningDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sponsor = table.Column<string>(type: "text", nullable: true),
                    Scope = table.Column<string>(type: "text", nullable: true),
                    Budget = table.Column<decimal>(type: "numeric", nullable: true),
                    DocumentUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissioningDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommissioningDocuments_systematic_review_projects_ProjectId",
                        column: x => x.ProjectId,
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
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_research_question", x => x.research_question_id);
                    table.ForeignKey(
                        name: "FK_research_question_QuestionTypes_question_type_id",
                        column: x => x.question_type_id,
                        principalTable: "QuestionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_research_question_systematic_review_projects_project_id",
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
                name: "ReviewNeeds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Justification = table.Column<string>(type: "text", nullable: true),
                    IdentifiedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewNeeds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewNeeds_systematic_review_projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReviewObjectives",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectiveStatement = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewObjectives", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewObjectives_systematic_review_projects_ProjectId",
                        column: x => x.ProjectId,
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
                name: "prisma_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_process_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                        name: "FK_prisma_reports_review_processes_review_process_id",
                        column: x => x.review_process_id,
                        principalTable: "review_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "study_selection_processes",
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
                    table.PrimaryKey("PK_study_selection_processes", x => x.id);
                    table.ForeignKey(
                        name: "FK_study_selection_processes_review_processes_review_process_id",
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
                name: "ProtocolEvaluations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProtocolId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewerId = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluationResult = table.Column<string>(type: "text", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    EvaluatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtocolEvaluations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtocolEvaluations_ProtocolReviewers_ReviewerId",
                        column: x => x.ReviewerId,
                        principalTable: "ProtocolReviewers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProtocolEvaluations_review_protocol_ProtocolId",
                        column: x => x.ProtocolId,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProtocolVersions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProtocolId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<string>(type: "text", nullable: false),
                    ChangeSummary = table.Column<string>(type: "text", nullable: true),
                    SnapshotData = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProtocolVersions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProtocolVersions_review_protocol_ProtocolId",
                        column: x => x.ProtocolId,
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
                name: "SearchSources",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProtocolId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceType = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchSources_review_protocol_ProtocolId",
                        column: x => x.ProtocolId,
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
                name: "StudySelectionCriterias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProtocolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudySelectionCriterias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudySelectionCriterias_review_protocol_ProtocolId",
                        column: x => x.ProtocolId,
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
                name: "SearchStrings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StrategyId = table.Column<Guid>(type: "uuid", nullable: false),
                    Expression = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SearchStrings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SearchStrings_search_strategy_StrategyId",
                        column: x => x.StrategyId,
                        principalTable: "search_strategy",
                        principalColumn: "strategy_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BibliographicDatabases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BibliographicDatabases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BibliographicDatabases_SearchSources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "SearchSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConferenceProceedings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConferenceProceedings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConferenceProceedings_SearchSources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "SearchSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DigitalLibraries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DigitalLibraries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DigitalLibraries_SearchSources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "SearchSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Journals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Journals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Journals_SearchSources_SourceId",
                        column: x => x.SourceId,
                        principalTable: "SearchSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExclusionCriteria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CriteriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rule = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExclusionCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExclusionCriteria_StudySelectionCriterias_CriteriaId",
                        column: x => x.CriteriaId,
                        principalTable: "StudySelectionCriterias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InclusionCriteria",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CriteriaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rule = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InclusionCriteria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InclusionCriteria_StudySelectionCriterias_CriteriaId",
                        column: x => x.CriteriaId,
                        principalTable: "StudySelectionCriterias",
                        principalColumn: "Id",
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
                        name: "FK_search_string_term_SearchStrings_search_string_id",
                        column: x => x.search_string_id,
                        principalTable: "SearchStrings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_search_string_term_SearchTerms_term_id",
                        column: x => x.term_id,
                        principalTable: "SearchTerms",
                        principalColumn: "Id",
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
                    last_decision_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    internal_notes = table.Column<string>(type: "text", nullable: true),
                    is_duplicate = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    duplicate_of_id = table.Column<Guid>(type: "uuid", nullable: true),
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
                        name: "FK_papers_papers_duplicate_of_id",
                        column: x => x.duplicate_of_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_papers_systematic_review_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "screening_decisions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    study_selection_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    decision = table.Column<string>(type: "text", nullable: false),
                    reason = table.Column<string>(type: "text", nullable: true),
                    decided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_screening_decisions", x => x.id);
                    table.ForeignKey(
                        name: "FK_screening_decisions_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_screening_decisions_study_selection_processes_study_selecti~",
                        column: x => x.study_selection_process_id,
                        principalTable: "study_selection_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "screening_resolutions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    study_selection_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    final_decision = table.Column<string>(type: "text", nullable: false),
                    resolution_notes = table.Column<string>(type: "text", nullable: true),
                    resolved_by = table.Column<Guid>(type: "uuid", nullable: false),
                    resolved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_screening_resolutions", x => x.id);
                    table.ForeignKey(
                        name: "FK_screening_resolutions_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_screening_resolutions_study_selection_processes_study_selec~",
                        column: x => x.study_selection_process_id,
                        principalTable: "study_selection_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BibliographicDatabases_SourceId",
                table: "BibliographicDatabases",
                column: "SourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissioningDocuments_ProjectId",
                table: "CommissioningDocuments",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_comparison_picoc_id",
                table: "comparison",
                column: "picoc_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConferenceProceedings_SourceId",
                table: "ConferenceProceedings",
                column: "SourceId",
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
                name: "IX_DigitalLibraries_SourceId",
                table: "DigitalLibraries",
                column: "SourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_dissemination_strategy_protocol_id",
                table: "dissemination_strategy",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_ExclusionCriteria_CriteriaId",
                table: "ExclusionCriteria",
                column: "CriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_identification_processes_review_process_id",
                table: "identification_processes",
                column: "review_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_import_batches_search_execution_id",
                table: "import_batches",
                column: "search_execution_id");

            migrationBuilder.CreateIndex(
                name: "IX_InclusionCriteria_CriteriaId",
                table: "InclusionCriteria",
                column: "CriteriaId");

            migrationBuilder.CreateIndex(
                name: "IX_intervention_picoc_id",
                table: "intervention",
                column: "picoc_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Journals_SourceId",
                table: "Journals",
                column: "SourceId",
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
                name: "IX_papers_duplicate_of_id",
                table: "papers",
                column: "duplicate_of_id");

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
                name: "IX_prisma_reports_review_process_id",
                table: "prisma_reports",
                column: "review_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_timetable_protocol_id",
                table: "project_timetable",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolEvaluations_ProtocolId",
                table: "ProtocolEvaluations",
                column: "ProtocolId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolEvaluations_ReviewerId",
                table: "ProtocolEvaluations",
                column: "ReviewerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProtocolVersions_ProtocolId",
                table: "ProtocolVersions",
                column: "ProtocolId");

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
                name: "IX_ReviewNeeds_ProjectId",
                table: "ReviewNeeds",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewObjectives_ProjectId",
                table: "ReviewObjectives",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_screening_decisions_paper_id",
                table: "screening_decisions",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_screening_decisions_reviewer_id",
                table: "screening_decisions",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "IX_screening_decisions_study_selection_process_id",
                table: "screening_decisions",
                column: "study_selection_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_screening_decisions_study_selection_process_id_paper_id_rev~",
                table: "screening_decisions",
                columns: new[] { "study_selection_process_id", "paper_id", "reviewer_id" });

            migrationBuilder.CreateIndex(
                name: "IX_screening_resolutions_paper_id",
                table: "screening_resolutions",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_screening_resolutions_resolved_by",
                table: "screening_resolutions",
                column: "resolved_by");

            migrationBuilder.CreateIndex(
                name: "IX_screening_resolutions_study_selection_process_id_paper_id",
                table: "screening_resolutions",
                columns: new[] { "study_selection_process_id", "paper_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_search_executions_identification_process_id",
                table: "search_executions",
                column: "identification_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_search_strategy_protocol_id",
                table: "search_strategy",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_search_string_term_term_id",
                table: "search_string_term",
                column: "term_id");

            migrationBuilder.CreateIndex(
                name: "IX_SearchSources_ProtocolId",
                table: "SearchSources",
                column: "ProtocolId");

            migrationBuilder.CreateIndex(
                name: "IX_SearchStrings_StrategyId",
                table: "SearchStrings",
                column: "StrategyId");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_procedure_protocol_id",
                table: "study_selection_procedure",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_processes_review_process_id",
                table: "study_selection_processes",
                column: "review_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_StudySelectionCriterias_ProtocolId",
                table: "StudySelectionCriterias",
                column: "ProtocolId");

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
                name: "BibliographicDatabases");

            migrationBuilder.DropTable(
                name: "CommissioningDocuments");

            migrationBuilder.DropTable(
                name: "comparison");

            migrationBuilder.DropTable(
                name: "ConferenceProceedings");

            migrationBuilder.DropTable(
                name: "context");

            migrationBuilder.DropTable(
                name: "data_item_definition");

            migrationBuilder.DropTable(
                name: "data_synthesis_strategy");

            migrationBuilder.DropTable(
                name: "DigitalLibraries");

            migrationBuilder.DropTable(
                name: "dissemination_strategy");

            migrationBuilder.DropTable(
                name: "ExclusionCriteria");

            migrationBuilder.DropTable(
                name: "InclusionCriteria");

            migrationBuilder.DropTable(
                name: "intervention");

            migrationBuilder.DropTable(
                name: "Journals");

            migrationBuilder.DropTable(
                name: "outcome");

            migrationBuilder.DropTable(
                name: "population");

            migrationBuilder.DropTable(
                name: "prisma_flow_records");

            migrationBuilder.DropTable(
                name: "project_timetable");

            migrationBuilder.DropTable(
                name: "ProtocolEvaluations");

            migrationBuilder.DropTable(
                name: "ProtocolVersions");

            migrationBuilder.DropTable(
                name: "quality_criterion");

            migrationBuilder.DropTable(
                name: "ReviewNeeds");

            migrationBuilder.DropTable(
                name: "ReviewObjectives");

            migrationBuilder.DropTable(
                name: "screening_decisions");

            migrationBuilder.DropTable(
                name: "screening_resolutions");

            migrationBuilder.DropTable(
                name: "search_string_term");

            migrationBuilder.DropTable(
                name: "study_selection_procedure");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "data_extraction_form");

            migrationBuilder.DropTable(
                name: "StudySelectionCriterias");

            migrationBuilder.DropTable(
                name: "SearchSources");

            migrationBuilder.DropTable(
                name: "picoc_element");

            migrationBuilder.DropTable(
                name: "prisma_reports");

            migrationBuilder.DropTable(
                name: "ProtocolReviewers");

            migrationBuilder.DropTable(
                name: "quality_checklist");

            migrationBuilder.DropTable(
                name: "papers");

            migrationBuilder.DropTable(
                name: "study_selection_processes");

            migrationBuilder.DropTable(
                name: "SearchStrings");

            migrationBuilder.DropTable(
                name: "SearchTerms");

            migrationBuilder.DropTable(
                name: "data_extraction_strategy");

            migrationBuilder.DropTable(
                name: "research_question");

            migrationBuilder.DropTable(
                name: "quality_assessment_strategy");

            migrationBuilder.DropTable(
                name: "import_batches");

            migrationBuilder.DropTable(
                name: "search_strategy");

            migrationBuilder.DropTable(
                name: "QuestionTypes");

            migrationBuilder.DropTable(
                name: "search_executions");

            migrationBuilder.DropTable(
                name: "review_protocol");

            migrationBuilder.DropTable(
                name: "identification_processes");

            migrationBuilder.DropTable(
                name: "review_processes");

            migrationBuilder.DropTable(
                name: "systematic_review_projects");
        }
    }
}
