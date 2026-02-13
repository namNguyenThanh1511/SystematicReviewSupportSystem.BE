using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class TestCheckSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.AlterDatabase()
				.Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

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
                name: "systematic_review_project",
                columns: table => new
                {
                    project_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
                    title = table.Column<string>(type: "text", nullable: false),
                    domain = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    modified_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_systematic_review_project", x => x.project_id);
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
                        name: "FK_CommissioningDocuments_systematic_review_project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "systematic_review_project",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "research_question",
                columns: table => new
                {
                    research_question_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
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
                        name: "FK_research_question_systematic_review_project_project_id",
                        column: x => x.project_id,
                        principalTable: "systematic_review_project",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "review_protocol",
                columns: table => new
                {
                    protocol_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
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
                        name: "FK_review_protocol_systematic_review_project_project_id",
                        column: x => x.project_id,
                        principalTable: "systematic_review_project",
                        principalColumn: "project_id",
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
                        name: "FK_ReviewNeeds_systematic_review_project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "systematic_review_project",
                        principalColumn: "project_id",
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
                        name: "FK_ReviewObjectives_systematic_review_project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "systematic_review_project",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "picoc_element",
                columns: table => new
                {
                    picoc_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
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
                name: "search_strategy",
                columns: table => new
                {
                    strategy_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
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
                    comparison_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
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
                    context_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
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
                    intervention_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
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
                    outcome_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
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
                    population_id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuid_generate_v4()"),
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
                name: "search_string_term",
                columns: table => new
                {
                    search_string_id = table.Column<Guid>(type: "uuid", nullable: false),
                    term_id = table.Column<Guid>(type: "uuid", nullable: false)
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
                name: "IX_DigitalLibraries_SourceId",
                table: "DigitalLibraries",
                column: "SourceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExclusionCriteria_CriteriaId",
                table: "ExclusionCriteria",
                column: "CriteriaId");

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
                name: "IX_picoc_element_research_question_id",
                table: "picoc_element",
                column: "research_question_id");

            migrationBuilder.CreateIndex(
                name: "IX_population_picoc_id",
                table: "population",
                column: "picoc_id",
                unique: true);

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
                name: "IX_research_question_project_id",
                table: "research_question",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_research_question_question_type_id",
                table: "research_question",
                column: "question_type_id");

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
                name: "IX_StudySelectionCriterias_ProtocolId",
                table: "StudySelectionCriterias",
                column: "ProtocolId");
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
                name: "DigitalLibraries");

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
                name: "ProtocolEvaluations");

            migrationBuilder.DropTable(
                name: "ProtocolVersions");

            migrationBuilder.DropTable(
                name: "ReviewNeeds");

            migrationBuilder.DropTable(
                name: "ReviewObjectives");

            migrationBuilder.DropTable(
                name: "search_string_term");

            migrationBuilder.DropTable(
                name: "StudySelectionCriterias");

            migrationBuilder.DropTable(
                name: "SearchSources");

            migrationBuilder.DropTable(
                name: "picoc_element");

            migrationBuilder.DropTable(
                name: "ProtocolReviewers");

            migrationBuilder.DropTable(
                name: "SearchStrings");

            migrationBuilder.DropTable(
                name: "SearchTerms");

            migrationBuilder.DropTable(
                name: "research_question");

            migrationBuilder.DropTable(
                name: "search_strategy");

            migrationBuilder.DropTable(
                name: "QuestionTypes");

            migrationBuilder.DropTable(
                name: "review_protocol");

            migrationBuilder.DropTable(
                name: "systematic_review_project");
        }
    }
}
