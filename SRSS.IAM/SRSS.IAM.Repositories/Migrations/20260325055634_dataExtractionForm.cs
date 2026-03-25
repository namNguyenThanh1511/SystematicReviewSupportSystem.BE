using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class dataExtractionForm : Migration
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
                name: "review_protocol",
                columns: table => new
                {
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocol_version = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    approved_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                name: "notifications",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    related_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entity_type = table.Column<int>(type: "integer", nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    metadata = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                    table.ForeignKey(
                        name: "FK_notifications_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_member_invitations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    invited_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    response_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    expired_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    responded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_member_invitations", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_member_invitations_systematic_review_projects_proje~",
                        column: x => x.project_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_member_invitations_users_invited_by_user_id",
                        column: x => x.invited_by_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_member_invitations_users_invited_user_id",
                        column: x => x.invited_user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_members",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    joined_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "timezone('utc', now())"),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_members", x => x.id);
                    table.ForeignKey(
                        name: "FK_project_members_systematic_review_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_members_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
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
                name: "extraction_template",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProtocolId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extraction_template", x => x.Id);
                    table.ForeignKey(
                        name: "FK_extraction_template_review_protocol_ProtocolId",
                        column: x => x.ProtocolId,
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
                    planned_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                name: "review_processes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
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
                        name: "FK_review_processes_review_protocol_protocol_id",
                        column: x => x.protocol_id,
                        principalTable: "review_protocol",
                        principalColumn: "protocol_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_review_processes_systematic_review_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "search_source",
                columns: table => new
                {
                    source_id = table.Column<Guid>(type: "uuid", nullable: false),
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                name: "extraction_section",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    SectionType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extraction_section", x => x.Id);
                    table.ForeignKey(
                        name: "FK_extraction_section_extraction_template_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "extraction_template",
                        principalColumn: "Id",
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
                name: "data_extraction_process",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    review_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_data_extraction_process", x => x.id);
                    table.ForeignKey(
                        name: "FK_data_extraction_process_review_processes_review_process_id",
                        column: x => x.review_process_id,
                        principalTable: "review_processes",
                        principalColumn: "id",
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
                    CurrentPhase = table.Column<int>(type: "integer", nullable: false),
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
                name: "extraction_field",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParentFieldId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Instruction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    FieldType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extraction_field", x => x.Id);
                    table.ForeignKey(
                        name: "FK_extraction_field_extraction_field_ParentFieldId",
                        column: x => x.ParentFieldId,
                        principalTable: "extraction_field",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_extraction_field_extraction_section_SectionId",
                        column: x => x.SectionId,
                        principalTable: "extraction_section",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExtractionMatrixColumns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExtractionMatrixColumns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExtractionMatrixColumns_extraction_section_SectionId",
                        column: x => x.SectionId,
                        principalTable: "extraction_section",
                        principalColumn: "Id",
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
                    metadata_json = table.Column<string>(type: "jsonb", nullable: true),
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
                name: "full_text_screenings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    study_selection_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    min_reviewers_per_paper = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    max_reviewers_per_paper = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_full_text_screenings", x => x.id);
                    table.ForeignKey(
                        name: "FK_full_text_screenings_study_selection_processes_study_select~",
                        column: x => x.study_selection_process_id,
                        principalTable: "study_selection_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "title_abstract_screenings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    study_selection_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    min_reviewers_per_paper = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    max_reviewers_per_paper = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_title_abstract_screenings", x => x.id);
                    table.ForeignKey(
                        name: "FK_title_abstract_screenings_study_selection_processes_study_s~",
                        column: x => x.study_selection_process_id,
                        principalTable: "study_selection_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "field_option",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_field_option", x => x.Id);
                    table.ForeignKey(
                        name: "FK_field_option_extraction_field_FieldId",
                        column: x => x.FieldId,
                        principalTable: "extraction_field",
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
                    Md5 = table.Column<string>(type: "text", nullable: true),
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
                    SourceType = table.Column<int>(type: "integer", nullable: false),
                    import_batch_id = table.Column<Guid>(type: "uuid", nullable: true),
                    review_process_id = table.Column<Guid>(type: "uuid", nullable: true),
                    imported_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    imported_by = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    pdf_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PdfFileName = table.Column<string>(type: "text", nullable: true),
                    full_text_available = table.Column<bool>(type: "boolean", nullable: true),
                    access_type = table.Column<string>(type: "text", nullable: true),
                    internal_notes = table.Column<string>(type: "text", nullable: true),
                    open_alex_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    external_citation_count = table.Column<int>(type: "integer", nullable: true),
                    external_reference_count = table.Column<int>(type: "integer", nullable: true),
                    external_cited_by_percentile = table.Column<double>(type: "double precision", nullable: true),
                    external_last_fetched_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    external_source = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    external_data_fetched = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                        name: "FK_papers_review_processes_review_process_id",
                        column: x => x.review_process_id,
                        principalTable: "review_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_papers_systematic_review_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "systematic_review_projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CandidatePapers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReviewProcessId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginPaperId = table.Column<Guid>(type: "uuid", nullable: true),
                    Title = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Authors = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PublicationYear = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DOI = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    RawReference = table.Column<string>(type: "text", nullable: true),
                    NormalizedReference = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CandidatePapers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CandidatePapers_papers_OriginPaperId",
                        column: x => x.OriginPaperId,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CandidatePapers_review_processes_ReviewProcessId",
                        column: x => x.ReviewProcessId,
                        principalTable: "review_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "deduplication_results",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identification_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    duplicate_of_paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    method = table.Column<string>(type: "text", nullable: false),
                    confidence_score = table.Column<decimal>(type: "numeric(5,4)", precision: 5, scale: 4, nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    review_status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    reviewed_by = table.Column<string>(type: "text", nullable: true),
                    reviewed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    resolved_decision = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deduplication_results", x => x.id);
                    table.CheckConstraint("ck_deduplication_no_self_duplicate", "paper_id != duplicate_of_paper_id");
                    table.ForeignKey(
                        name: "FK_deduplication_results_identification_processes_identificati~",
                        column: x => x.identification_process_id,
                        principalTable: "identification_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deduplication_results_papers_duplicate_of_paper_id",
                        column: x => x.duplicate_of_paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deduplication_results_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "extracted_data_value",
                columns: table => new
                {
                    value_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    field_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    option_id = table.Column<Guid>(type: "uuid", nullable: true),
                    string_value = table.Column<string>(type: "text", nullable: true),
                    numeric_value = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: true),
                    boolean_value = table.Column<bool>(type: "boolean", nullable: true),
                    matrix_column_id = table.Column<Guid>(type: "uuid", nullable: true),
                    matrix_row_index = table.Column<int>(type: "integer", nullable: true),
                    is_consensus_final = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extracted_data_value", x => x.value_id);
                    table.ForeignKey(
                        name: "FK_extracted_data_value_ExtractionMatrixColumns_matrix_column_~",
                        column: x => x.matrix_column_id,
                        principalTable: "ExtractionMatrixColumns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_extracted_data_value_extraction_field_field_id",
                        column: x => x.field_id,
                        principalTable: "extraction_field",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_extracted_data_value_field_option_option_id",
                        column: x => x.option_id,
                        principalTable: "field_option",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_extracted_data_value_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_extracted_data_value_users_reviewer_id",
                        column: x => x.reviewer_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "extraction_paper_task",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    data_extraction_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    reviewer_1_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewer_2_id = table.Column<Guid>(type: "uuid", nullable: true),
                    adjudicator_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reviewer_1_status = table.Column<string>(type: "text", nullable: false),
                    reviewer_2_status = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_extraction_paper_task", x => x.id);
                    table.ForeignKey(
                        name: "FK_extraction_paper_task_data_extraction_process_data_extracti~",
                        column: x => x.data_extraction_process_id,
                        principalTable: "data_extraction_process",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_extraction_paper_task_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_extraction_paper_task_users_adjudicator_id",
                        column: x => x.adjudicator_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_extraction_paper_task_users_reviewer_1_id",
                        column: x => x.reviewer_1_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_extraction_paper_task_users_reviewer_2_id",
                        column: x => x.reviewer_2_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "identification_process_papers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    identification_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    included_after_dedup = table.Column<bool>(type: "boolean", nullable: false),
                    source_type = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_identification_process_papers", x => x.id);
                    table.ForeignKey(
                        name: "FK_identification_process_papers_identification_processes_iden~",
                        column: x => x.identification_process_id,
                        principalTable: "identification_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_identification_process_papers_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "paper_assignments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_member_id = table.Column<Guid>(type: "uuid", nullable: false),
                    study_selection_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    Phase = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paper_assignments", x => x.id);
                    table.ForeignKey(
                        name: "FK_paper_assignments_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_paper_assignments_project_members_project_member_id",
                        column: x => x.project_member_id,
                        principalTable: "project_members",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_paper_assignments_study_selection_processes_study_selection~",
                        column: x => x.study_selection_process_id,
                        principalTable: "study_selection_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "paper_citations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    raw_reference = table.Column<string>(type: "text", nullable: true),
                    confidence_score = table.Column<decimal>(type: "numeric", nullable: false),
                    source = table.Column<string>(type: "text", nullable: false),
                    is_external = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    external_id = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    weight = table.Column<decimal>(type: "numeric", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paper_citations", x => x.id);
                    table.ForeignKey(
                        name: "FK_paper_citations_papers_source_paper_id",
                        column: x => x.source_paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_paper_citations_papers_target_paper_id",
                        column: x => x.target_paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "paper_embeddings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    embedding = table.Column<float[]>(type: "real[]", nullable: false),
                    model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paper_embeddings", x => x.id);
                    table.ForeignKey(
                        name: "FK_paper_embeddings_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "paper_pdfs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaperId = table.Column<Guid>(type: "uuid", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GrobidProcessed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paper_pdfs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_paper_pdfs_papers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "paper_source_metadatas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaperId = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Authors = table.Column<string>(type: "text", nullable: true),
                    Abstract = table.Column<string>(type: "text", nullable: true),
                    DOI = table.Column<string>(type: "text", nullable: true),
                    Journal = table.Column<string>(type: "text", nullable: true),
                    Volume = table.Column<string>(type: "text", nullable: true),
                    Issue = table.Column<string>(type: "text", nullable: true),
                    Pages = table.Column<string>(type: "text", nullable: true),
                    Keywords = table.Column<string>(type: "text", nullable: true),
                    Publisher = table.Column<string>(type: "text", nullable: true),
                    PublishedDate = table.Column<string>(type: "text", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    ISSN = table.Column<string>(type: "text", nullable: true),
                    EISSN = table.Column<string>(type: "text", nullable: true),
                    Language = table.Column<string>(type: "text", nullable: true),
                    Md5 = table.Column<string>(type: "text", nullable: true),
                    AppliedFields = table.Column<List<string>>(type: "text[]", nullable: false),
                    ExtractedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paper_source_metadatas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_paper_source_metadatas_papers_PaperId",
                        column: x => x.PaperId,
                        principalTable: "papers",
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
                    screening_phase = table.Column<string>(type: "text", nullable: false),
                    exclusion_reason_code = table.Column<string>(type: "text", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: true),
                    reviewer_notes = table.Column<string>(type: "text", nullable: true),
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
                    screening_phase = table.Column<string>(type: "text", nullable: false),
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

            migrationBuilder.CreateTable(
                name: "study_selection_process_papers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    study_selection_process_id = table.Column<Guid>(type: "uuid", nullable: false),
                    paper_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_study_selection_process_papers", x => x.id);
                    table.ForeignKey(
                        name: "FK_study_selection_process_papers_papers_paper_id",
                        column: x => x.paper_id,
                        principalTable: "papers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_study_selection_process_papers_study_selection_processes_st~",
                        column: x => x.study_selection_process_id,
                        principalTable: "study_selection_processes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grobid_header_results",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaperPdfId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: true),
                    Authors = table.Column<string>(type: "text", nullable: true),
                    Abstract = table.Column<string>(type: "text", nullable: true),
                    DOI = table.Column<string>(type: "text", nullable: true),
                    Journal = table.Column<string>(type: "text", nullable: true),
                    Volume = table.Column<string>(type: "text", nullable: true),
                    Issue = table.Column<string>(type: "text", nullable: true),
                    Pages = table.Column<string>(type: "text", nullable: true),
                    RawXml = table.Column<string>(type: "text", nullable: true),
                    ExtractedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grobid_header_results", x => x.Id);
                    table.ForeignKey(
                        name: "FK_grobid_header_results_paper_pdfs_PaperPdfId",
                        column: x => x.PaperPdfId,
                        principalTable: "paper_pdfs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CandidatePapers_OriginPaperId",
                table: "CandidatePapers",
                column: "OriginPaperId");

            migrationBuilder.CreateIndex(
                name: "IX_CandidatePapers_ReviewProcessId",
                table: "CandidatePapers",
                column: "ReviewProcessId");

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
                name: "IX_context_picoc_id",
                table: "context",
                column: "picoc_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_data_extraction_form_extraction_strategy_id",
                table: "data_extraction_form",
                column: "extraction_strategy_id");

            migrationBuilder.CreateIndex(
                name: "IX_data_extraction_process_review_process_id",
                table: "data_extraction_process",
                column: "review_process_id",
                unique: true);

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
                name: "IX_deduplication_results_duplicate_of_paper_id",
                table: "deduplication_results",
                column: "duplicate_of_paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_deduplication_results_identification_process_id",
                table: "deduplication_results",
                column: "identification_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_deduplication_results_method",
                table: "deduplication_results",
                column: "method");

            migrationBuilder.CreateIndex(
                name: "IX_deduplication_results_paper_id",
                table: "deduplication_results",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_deduplication_results_review_status",
                table: "deduplication_results",
                column: "review_status");

            migrationBuilder.CreateIndex(
                name: "uq_deduplication_process_paper",
                table: "deduplication_results",
                columns: new[] { "identification_process_id", "paper_id" },
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
                name: "IX_extracted_data_value_field_id",
                table: "extracted_data_value",
                column: "field_id");

            migrationBuilder.CreateIndex(
                name: "IX_extracted_data_value_matrix_column_id",
                table: "extracted_data_value",
                column: "matrix_column_id");

            migrationBuilder.CreateIndex(
                name: "IX_extracted_data_value_option_id",
                table: "extracted_data_value",
                column: "option_id");

            migrationBuilder.CreateIndex(
                name: "IX_extracted_data_value_paper_id",
                table: "extracted_data_value",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_extracted_data_value_paper_id_field_id",
                table: "extracted_data_value",
                columns: new[] { "paper_id", "field_id" });

            migrationBuilder.CreateIndex(
                name: "IX_extracted_data_value_reviewer_id",
                table: "extracted_data_value",
                column: "reviewer_id");

            migrationBuilder.CreateIndex(
                name: "IX_extraction_field_ParentFieldId",
                table: "extraction_field",
                column: "ParentFieldId");

            migrationBuilder.CreateIndex(
                name: "IX_extraction_field_SectionId",
                table: "extraction_field",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_extraction_paper_task_adjudicator_id",
                table: "extraction_paper_task",
                column: "adjudicator_id");

            migrationBuilder.CreateIndex(
                name: "IX_extraction_paper_task_data_extraction_process_id",
                table: "extraction_paper_task",
                column: "data_extraction_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_extraction_paper_task_paper_id",
                table: "extraction_paper_task",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_extraction_paper_task_reviewer_1_id",
                table: "extraction_paper_task",
                column: "reviewer_1_id");

            migrationBuilder.CreateIndex(
                name: "IX_extraction_paper_task_reviewer_2_id",
                table: "extraction_paper_task",
                column: "reviewer_2_id");

            migrationBuilder.CreateIndex(
                name: "IX_extraction_section_TemplateId",
                table: "extraction_section",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_extraction_template_ProtocolId",
                table: "extraction_template",
                column: "ProtocolId");

            migrationBuilder.CreateIndex(
                name: "IX_ExtractionMatrixColumns_SectionId",
                table: "ExtractionMatrixColumns",
                column: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_field_option_FieldId",
                table: "field_option",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "idx_ft_screening_study_selection_process_id_unique",
                table: "full_text_screenings",
                column: "study_selection_process_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_grobid_header_results_PaperPdfId",
                table: "grobid_header_results",
                column: "PaperPdfId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_identification_process_papers_identification_process_id",
                table: "identification_process_papers",
                column: "identification_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_identification_process_papers_included_after_dedup",
                table: "identification_process_papers",
                column: "included_after_dedup");

            migrationBuilder.CreateIndex(
                name: "IX_identification_process_papers_paper_id",
                table: "identification_process_papers",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "uq_identification_process_paper",
                table: "identification_process_papers",
                columns: new[] { "identification_process_id", "paper_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_identification_process_review_process_id_unique",
                table: "identification_processes",
                column: "review_process_id",
                unique: true);

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
                name: "ix_notifications_is_read",
                table: "notifications",
                column: "is_read");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_outcome_picoc_id",
                table: "outcome",
                column: "picoc_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_paper_assignments_paper_id",
                table: "paper_assignments",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_paper_assignments_project_member_id",
                table: "paper_assignments",
                column: "project_member_id");

            migrationBuilder.CreateIndex(
                name: "IX_paper_assignments_study_selection_process_id",
                table: "paper_assignments",
                column: "study_selection_process_id");

            migrationBuilder.CreateIndex(
                name: "uq_paper_assignment_paper_member_process_phase",
                table: "paper_assignments",
                columns: new[] { "paper_id", "project_member_id", "study_selection_process_id", "Phase" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_paper_citations_source_paper_id",
                table: "paper_citations",
                column: "source_paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_paper_citations_source_paper_id_target_paper_id",
                table: "paper_citations",
                columns: new[] { "source_paper_id", "target_paper_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_paper_citations_target_paper_id",
                table: "paper_citations",
                column: "target_paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_paper_embeddings_paper_id",
                table: "paper_embeddings",
                column: "paper_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_paper_pdfs_PaperId",
                table: "paper_pdfs",
                column: "PaperId");

            migrationBuilder.CreateIndex(
                name: "IX_paper_source_metadatas_PaperId",
                table: "paper_source_metadatas",
                column: "PaperId");

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
                name: "IX_papers_review_process_id",
                table: "papers",
                column: "review_process_id");

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
                name: "IX_project_member_invitations_invited_by_user_id",
                table: "project_member_invitations",
                column: "invited_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_member_invitations_invited_user_id",
                table: "project_member_invitations",
                column: "invited_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_member_invitations_project_id",
                table: "project_member_invitations",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_member_invitations_status",
                table: "project_member_invitations",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_project_members_project_id",
                table: "project_members",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_members_project_id_role",
                table: "project_members",
                columns: new[] { "project_id", "role" },
                unique: true,
                filter: "role = 1");

            migrationBuilder.CreateIndex(
                name: "IX_project_members_project_id_user_id",
                table: "project_members",
                columns: new[] { "project_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_project_members_user_id",
                table: "project_members",
                column: "user_id");

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
                name: "IX_review_processes_protocol_id",
                table: "review_processes",
                column: "protocol_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_review_protocol_project_id",
                table: "review_protocol",
                column: "project_id");

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
                name: "uq_screening_decision_process_paper_reviewer_phase",
                table: "screening_decisions",
                columns: new[] { "study_selection_process_id", "paper_id", "reviewer_id", "screening_phase" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_screening_resolutions_paper_id",
                table: "screening_resolutions",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_screening_resolutions_resolved_by",
                table: "screening_resolutions",
                column: "resolved_by");

            migrationBuilder.CreateIndex(
                name: "uq_screening_resolution_process_paper_phase",
                table: "screening_resolutions",
                columns: new[] { "study_selection_process_id", "paper_id", "screening_phase" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_search_executions_identification_process_id",
                table: "search_executions",
                column: "identification_process_id");

            migrationBuilder.CreateIndex(
                name: "IX_search_source_protocol_id",
                table: "search_source",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_criteria_protocol_id",
                table: "study_selection_criteria",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_procedure_protocol_id",
                table: "study_selection_procedure",
                column: "protocol_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_process_papers_paper_id",
                table: "study_selection_process_papers",
                column: "paper_id");

            migrationBuilder.CreateIndex(
                name: "IX_study_selection_process_papers_study_selection_process_id",
                table: "study_selection_process_papers",
                column: "study_selection_process_id");

            migrationBuilder.CreateIndex(
                name: "uq_study_selection_process_paper",
                table: "study_selection_process_papers",
                columns: new[] { "study_selection_process_id", "paper_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_study_selection_process_review_process_id_unique",
                table: "study_selection_processes",
                column: "review_process_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_project_status",
                table: "systematic_review_projects",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_project_title",
                table: "systematic_review_projects",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "idx_ta_screening_study_selection_process_id_unique",
                table: "title_abstract_screenings",
                column: "study_selection_process_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_full_name",
                table: "users",
                column: "full_name");

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
                name: "CandidatePapers");

            migrationBuilder.DropTable(
                name: "commissioning_document");

            migrationBuilder.DropTable(
                name: "comparison");

            migrationBuilder.DropTable(
                name: "context");

            migrationBuilder.DropTable(
                name: "data_item_definition");

            migrationBuilder.DropTable(
                name: "data_synthesis_strategy");

            migrationBuilder.DropTable(
                name: "deduplication_results");

            migrationBuilder.DropTable(
                name: "dissemination_strategy");

            migrationBuilder.DropTable(
                name: "exclusion_criterion");

            migrationBuilder.DropTable(
                name: "extracted_data_value");

            migrationBuilder.DropTable(
                name: "extraction_paper_task");

            migrationBuilder.DropTable(
                name: "full_text_screenings");

            migrationBuilder.DropTable(
                name: "grobid_header_results");

            migrationBuilder.DropTable(
                name: "identification_process_papers");

            migrationBuilder.DropTable(
                name: "inclusion_criterion");

            migrationBuilder.DropTable(
                name: "intervention");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "outcome");

            migrationBuilder.DropTable(
                name: "paper_assignments");

            migrationBuilder.DropTable(
                name: "paper_citations");

            migrationBuilder.DropTable(
                name: "paper_embeddings");

            migrationBuilder.DropTable(
                name: "paper_source_metadatas");

            migrationBuilder.DropTable(
                name: "population");

            migrationBuilder.DropTable(
                name: "prisma_flow_records");

            migrationBuilder.DropTable(
                name: "project_member_invitations");

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
                name: "screening_decisions");

            migrationBuilder.DropTable(
                name: "screening_resolutions");

            migrationBuilder.DropTable(
                name: "search_source");

            migrationBuilder.DropTable(
                name: "study_selection_procedure");

            migrationBuilder.DropTable(
                name: "study_selection_process_papers");

            migrationBuilder.DropTable(
                name: "title_abstract_screenings");

            migrationBuilder.DropTable(
                name: "data_extraction_form");

            migrationBuilder.DropTable(
                name: "ExtractionMatrixColumns");

            migrationBuilder.DropTable(
                name: "field_option");

            migrationBuilder.DropTable(
                name: "data_extraction_process");

            migrationBuilder.DropTable(
                name: "paper_pdfs");

            migrationBuilder.DropTable(
                name: "study_selection_criteria");

            migrationBuilder.DropTable(
                name: "project_members");

            migrationBuilder.DropTable(
                name: "picoc_element");

            migrationBuilder.DropTable(
                name: "prisma_reports");

            migrationBuilder.DropTable(
                name: "protocol_reviewer");

            migrationBuilder.DropTable(
                name: "quality_checklist");

            migrationBuilder.DropTable(
                name: "study_selection_processes");

            migrationBuilder.DropTable(
                name: "data_extraction_strategy");

            migrationBuilder.DropTable(
                name: "extraction_field");

            migrationBuilder.DropTable(
                name: "papers");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "research_question");

            migrationBuilder.DropTable(
                name: "quality_assessment_strategy");

            migrationBuilder.DropTable(
                name: "extraction_section");

            migrationBuilder.DropTable(
                name: "import_batches");

            migrationBuilder.DropTable(
                name: "question_type");

            migrationBuilder.DropTable(
                name: "extraction_template");

            migrationBuilder.DropTable(
                name: "search_executions");

            migrationBuilder.DropTable(
                name: "identification_processes");

            migrationBuilder.DropTable(
                name: "review_processes");

            migrationBuilder.DropTable(
                name: "review_protocol");

            migrationBuilder.DropTable(
                name: "systematic_review_projects");
        }
    }
}
