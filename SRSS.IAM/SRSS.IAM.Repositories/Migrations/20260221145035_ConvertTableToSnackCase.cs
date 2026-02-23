using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRSS.IAM.Repositories.Migrations
{
    /// <inheritdoc />
    public partial class ConvertTableToSnackCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BibliographicDatabases_SearchSources_SourceId",
                table: "BibliographicDatabases");

            migrationBuilder.DropForeignKey(
                name: "FK_CommissioningDocuments_systematic_review_projects_ProjectId",
                table: "CommissioningDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_ConferenceProceedings_SearchSources_SourceId",
                table: "ConferenceProceedings");

            migrationBuilder.DropForeignKey(
                name: "FK_DigitalLibraries_SearchSources_SourceId",
                table: "DigitalLibraries");

            migrationBuilder.DropForeignKey(
                name: "FK_ExclusionCriteria_StudySelectionCriterias_CriteriaId",
                table: "ExclusionCriteria");

            migrationBuilder.DropForeignKey(
                name: "FK_InclusionCriteria_StudySelectionCriterias_CriteriaId",
                table: "InclusionCriteria");

            migrationBuilder.DropForeignKey(
                name: "FK_Journals_SearchSources_SourceId",
                table: "Journals");

            migrationBuilder.DropForeignKey(
                name: "FK_ProtocolEvaluations_ProtocolReviewers_ReviewerId",
                table: "ProtocolEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_ProtocolEvaluations_review_protocol_ProtocolId",
                table: "ProtocolEvaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_ProtocolVersions_review_protocol_ProtocolId",
                table: "ProtocolVersions");

            migrationBuilder.DropForeignKey(
                name: "FK_research_question_QuestionTypes_question_type_id",
                table: "research_question");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewNeeds_systematic_review_projects_ProjectId",
                table: "ReviewNeeds");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewObjectives_systematic_review_projects_ProjectId",
                table: "ReviewObjectives");

            migrationBuilder.DropForeignKey(
                name: "FK_search_string_term_SearchStrings_search_string_id",
                table: "search_string_term");

            migrationBuilder.DropForeignKey(
                name: "FK_search_string_term_SearchTerms_term_id",
                table: "search_string_term");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchSources_review_protocol_ProtocolId",
                table: "SearchSources");

            migrationBuilder.DropForeignKey(
                name: "FK_SearchStrings_search_strategy_StrategyId",
                table: "SearchStrings");

            migrationBuilder.DropForeignKey(
                name: "FK_StudySelectionCriterias_review_protocol_ProtocolId",
                table: "StudySelectionCriterias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StudySelectionCriterias",
                table: "StudySelectionCriterias");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SearchTerms",
                table: "SearchTerms");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SearchStrings",
                table: "SearchStrings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SearchSources",
                table: "SearchSources");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReviewObjectives",
                table: "ReviewObjectives");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ReviewNeeds",
                table: "ReviewNeeds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuestionTypes",
                table: "QuestionTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProtocolVersions",
                table: "ProtocolVersions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProtocolReviewers",
                table: "ProtocolReviewers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProtocolEvaluations",
                table: "ProtocolEvaluations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Journals",
                table: "Journals");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InclusionCriteria",
                table: "InclusionCriteria");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ExclusionCriteria",
                table: "ExclusionCriteria");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DigitalLibraries",
                table: "DigitalLibraries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConferenceProceedings",
                table: "ConferenceProceedings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CommissioningDocuments",
                table: "CommissioningDocuments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BibliographicDatabases",
                table: "BibliographicDatabases");

            migrationBuilder.RenameTable(
                name: "StudySelectionCriterias",
                newName: "study_selection_criteria");

            migrationBuilder.RenameTable(
                name: "SearchTerms",
                newName: "search_term");

            migrationBuilder.RenameTable(
                name: "SearchStrings",
                newName: "search_string");

            migrationBuilder.RenameTable(
                name: "SearchSources",
                newName: "search_source");

            migrationBuilder.RenameTable(
                name: "ReviewObjectives",
                newName: "review_objective");

            migrationBuilder.RenameTable(
                name: "ReviewNeeds",
                newName: "review_need");

            migrationBuilder.RenameTable(
                name: "QuestionTypes",
                newName: "question_type");

            migrationBuilder.RenameTable(
                name: "ProtocolVersions",
                newName: "protocol_version");

            migrationBuilder.RenameTable(
                name: "ProtocolReviewers",
                newName: "protocol_reviewer");

            migrationBuilder.RenameTable(
                name: "ProtocolEvaluations",
                newName: "protocol_evaluation");

            migrationBuilder.RenameTable(
                name: "Journals",
                newName: "journal");

            migrationBuilder.RenameTable(
                name: "InclusionCriteria",
                newName: "inclusion_criterion");

            migrationBuilder.RenameTable(
                name: "ExclusionCriteria",
                newName: "exclusion_criterion");

            migrationBuilder.RenameTable(
                name: "DigitalLibraries",
                newName: "digital_library");

            migrationBuilder.RenameTable(
                name: "ConferenceProceedings",
                newName: "conference_proceeding");

            migrationBuilder.RenameTable(
                name: "CommissioningDocuments",
                newName: "commissioning_document");

            migrationBuilder.RenameTable(
                name: "BibliographicDatabases",
                newName: "bibliographic_database");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "research_question",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "study_selection_criteria",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "ProtocolId",
                table: "study_selection_criteria",
                newName: "protocol_id");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "study_selection_criteria",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "study_selection_criteria",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "study_selection_criteria",
                newName: "criteria_id");

            migrationBuilder.RenameIndex(
                name: "IX_StudySelectionCriterias_ProtocolId",
                table: "study_selection_criteria",
                newName: "IX_study_selection_criteria_protocol_id");

            migrationBuilder.RenameColumn(
                name: "Source",
                table: "search_term",
                newName: "source");

            migrationBuilder.RenameColumn(
                name: "Keyword",
                table: "search_term",
                newName: "keyword");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "search_term",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "search_term",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "search_term",
                newName: "term_id");

            migrationBuilder.RenameColumn(
                name: "Expression",
                table: "search_string",
                newName: "expression");

            migrationBuilder.RenameColumn(
                name: "StrategyId",
                table: "search_string",
                newName: "strategy_id");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "search_string",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "search_string",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "search_string",
                newName: "search_string_id");

            migrationBuilder.RenameIndex(
                name: "IX_SearchStrings_StrategyId",
                table: "search_string",
                newName: "IX_search_string_strategy_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "search_source",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "SourceType",
                table: "search_source",
                newName: "source_type");

            migrationBuilder.RenameColumn(
                name: "ProtocolId",
                table: "search_source",
                newName: "protocol_id");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "search_source",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "search_source",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "search_source",
                newName: "source_id");

            migrationBuilder.RenameIndex(
                name: "IX_SearchSources_ProtocolId",
                table: "search_source",
                newName: "IX_search_source_protocol_id");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "review_objective",
                newName: "project_id");

            migrationBuilder.RenameColumn(
                name: "ObjectiveStatement",
                table: "review_objective",
                newName: "objective_statement");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "review_objective",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "review_objective",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "review_objective",
                newName: "objective_id");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewObjectives_ProjectId",
                table: "review_objective",
                newName: "IX_review_objective_project_id");

            migrationBuilder.RenameColumn(
                name: "Justification",
                table: "review_need",
                newName: "justification");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "review_need",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "review_need",
                newName: "project_id");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "review_need",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "IdentifiedBy",
                table: "review_need",
                newName: "identified_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "review_need",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "review_need",
                newName: "need_id");

            migrationBuilder.RenameIndex(
                name: "IX_ReviewNeeds_ProjectId",
                table: "review_need",
                newName: "IX_review_need_project_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "question_type",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "question_type",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "question_type",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "question_type",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "question_type",
                newName: "question_type_id");

            migrationBuilder.RenameColumn(
                name: "VersionNumber",
                table: "protocol_version",
                newName: "version_number");

            migrationBuilder.RenameColumn(
                name: "SnapshotData",
                table: "protocol_version",
                newName: "snapshot_data");

            migrationBuilder.RenameColumn(
                name: "ProtocolId",
                table: "protocol_version",
                newName: "protocol_id");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "protocol_version",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "protocol_version",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "ChangeSummary",
                table: "protocol_version",
                newName: "change_summary");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "protocol_version",
                newName: "version_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProtocolVersions_ProtocolId",
                table: "protocol_version",
                newName: "IX_protocol_version_protocol_id");

            migrationBuilder.RenameColumn(
                name: "Role",
                table: "protocol_reviewer",
                newName: "role");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "protocol_reviewer",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Affiliation",
                table: "protocol_reviewer",
                newName: "affiliation");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "protocol_reviewer",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "protocol_reviewer",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "protocol_reviewer",
                newName: "reviewer_id");

            migrationBuilder.RenameColumn(
                name: "Comment",
                table: "protocol_evaluation",
                newName: "comment");

            migrationBuilder.RenameColumn(
                name: "ReviewerId",
                table: "protocol_evaluation",
                newName: "reviewer_id");

            migrationBuilder.RenameColumn(
                name: "ProtocolId",
                table: "protocol_evaluation",
                newName: "protocol_id");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "protocol_evaluation",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "EvaluationResult",
                table: "protocol_evaluation",
                newName: "evaluation_result");

            migrationBuilder.RenameColumn(
                name: "EvaluatedAt",
                table: "protocol_evaluation",
                newName: "evaluated_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "protocol_evaluation",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "protocol_evaluation",
                newName: "evaluation_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProtocolEvaluations_ReviewerId",
                table: "protocol_evaluation",
                newName: "IX_protocol_evaluation_reviewer_id");

            migrationBuilder.RenameIndex(
                name: "IX_ProtocolEvaluations_ProtocolId",
                table: "protocol_evaluation",
                newName: "IX_protocol_evaluation_protocol_id");

            migrationBuilder.RenameColumn(
                name: "SourceId",
                table: "journal",
                newName: "source_id");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "journal",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "journal",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "journal",
                newName: "journal_id");

            migrationBuilder.RenameIndex(
                name: "IX_Journals_SourceId",
                table: "journal",
                newName: "IX_journal_source_id");

            migrationBuilder.RenameColumn(
                name: "Rule",
                table: "inclusion_criterion",
                newName: "rule");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "inclusion_criterion",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CriteriaId",
                table: "inclusion_criterion",
                newName: "criteria_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "inclusion_criterion",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "inclusion_criterion",
                newName: "inclusion_id");

            migrationBuilder.RenameIndex(
                name: "IX_InclusionCriteria_CriteriaId",
                table: "inclusion_criterion",
                newName: "IX_inclusion_criterion_criteria_id");

            migrationBuilder.RenameColumn(
                name: "Rule",
                table: "exclusion_criterion",
                newName: "rule");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "exclusion_criterion",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CriteriaId",
                table: "exclusion_criterion",
                newName: "criteria_id");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "exclusion_criterion",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "exclusion_criterion",
                newName: "exclusion_id");

            migrationBuilder.RenameIndex(
                name: "IX_ExclusionCriteria_CriteriaId",
                table: "exclusion_criterion",
                newName: "IX_exclusion_criterion_criteria_id");

            migrationBuilder.RenameColumn(
                name: "SourceId",
                table: "digital_library",
                newName: "source_id");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "digital_library",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "digital_library",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "AccessUrl",
                table: "digital_library",
                newName: "access_url");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "digital_library",
                newName: "library_id");

            migrationBuilder.RenameIndex(
                name: "IX_DigitalLibraries_SourceId",
                table: "digital_library",
                newName: "IX_digital_library_source_id");

            migrationBuilder.RenameColumn(
                name: "SourceId",
                table: "conference_proceeding",
                newName: "source_id");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "conference_proceeding",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "conference_proceeding",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "conference_proceeding",
                newName: "conference_id");

            migrationBuilder.RenameIndex(
                name: "IX_ConferenceProceedings_SourceId",
                table: "conference_proceeding",
                newName: "IX_conference_proceeding_source_id");

            migrationBuilder.RenameColumn(
                name: "Sponsor",
                table: "commissioning_document",
                newName: "sponsor");

            migrationBuilder.RenameColumn(
                name: "Scope",
                table: "commissioning_document",
                newName: "scope");

            migrationBuilder.RenameColumn(
                name: "Budget",
                table: "commissioning_document",
                newName: "budget");

            migrationBuilder.RenameColumn(
                name: "ProjectId",
                table: "commissioning_document",
                newName: "project_id");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "commissioning_document",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "DocumentUrl",
                table: "commissioning_document",
                newName: "document_url");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "commissioning_document",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "commissioning_document",
                newName: "document_id");

            migrationBuilder.RenameIndex(
                name: "IX_CommissioningDocuments_ProjectId",
                table: "commissioning_document",
                newName: "IX_commissioning_document_project_id");

            migrationBuilder.RenameColumn(
                name: "SourceId",
                table: "bibliographic_database",
                newName: "source_id");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "bibliographic_database",
                newName: "modified_at");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "bibliographic_database",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "bibliographic_database",
                newName: "database_id");

            migrationBuilder.RenameIndex(
                name: "IX_BibliographicDatabases_SourceId",
                table: "bibliographic_database",
                newName: "IX_bibliographic_database_source_id");

            migrationBuilder.AddColumn<Guid>(
                name: "ProtocolReviewerId",
                table: "protocol_evaluation",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_study_selection_criteria",
                table: "study_selection_criteria",
                column: "criteria_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_search_term",
                table: "search_term",
                column: "term_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_search_string",
                table: "search_string",
                column: "search_string_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_search_source",
                table: "search_source",
                column: "source_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_review_objective",
                table: "review_objective",
                column: "objective_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_review_need",
                table: "review_need",
                column: "need_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_question_type",
                table: "question_type",
                column: "question_type_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_protocol_version",
                table: "protocol_version",
                column: "version_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_protocol_reviewer",
                table: "protocol_reviewer",
                column: "reviewer_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_protocol_evaluation",
                table: "protocol_evaluation",
                column: "evaluation_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_journal",
                table: "journal",
                column: "journal_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_inclusion_criterion",
                table: "inclusion_criterion",
                column: "inclusion_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_exclusion_criterion",
                table: "exclusion_criterion",
                column: "exclusion_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_digital_library",
                table: "digital_library",
                column: "library_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_conference_proceeding",
                table: "conference_proceeding",
                column: "conference_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_commissioning_document",
                table: "commissioning_document",
                column: "document_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_bibliographic_database",
                table: "bibliographic_database",
                column: "database_id");

            migrationBuilder.CreateIndex(
                name: "IX_protocol_evaluation_ProtocolReviewerId",
                table: "protocol_evaluation",
                column: "ProtocolReviewerId");

            migrationBuilder.AddForeignKey(
                name: "FK_bibliographic_database_search_source_source_id",
                table: "bibliographic_database",
                column: "source_id",
                principalTable: "search_source",
                principalColumn: "source_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_commissioning_document_systematic_review_projects_project_id",
                table: "commissioning_document",
                column: "project_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_conference_proceeding_search_source_source_id",
                table: "conference_proceeding",
                column: "source_id",
                principalTable: "search_source",
                principalColumn: "source_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_digital_library_search_source_source_id",
                table: "digital_library",
                column: "source_id",
                principalTable: "search_source",
                principalColumn: "source_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_exclusion_criterion_study_selection_criteria_criteria_id",
                table: "exclusion_criterion",
                column: "criteria_id",
                principalTable: "study_selection_criteria",
                principalColumn: "criteria_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_inclusion_criterion_study_selection_criteria_criteria_id",
                table: "inclusion_criterion",
                column: "criteria_id",
                principalTable: "study_selection_criteria",
                principalColumn: "criteria_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_journal_search_source_source_id",
                table: "journal",
                column: "source_id",
                principalTable: "search_source",
                principalColumn: "source_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_protocol_evaluation_protocol_reviewer_ProtocolReviewerId",
                table: "protocol_evaluation",
                column: "ProtocolReviewerId",
                principalTable: "protocol_reviewer",
                principalColumn: "reviewer_id");

            migrationBuilder.AddForeignKey(
                name: "FK_protocol_evaluation_protocol_reviewer_reviewer_id",
                table: "protocol_evaluation",
                column: "reviewer_id",
                principalTable: "protocol_reviewer",
                principalColumn: "reviewer_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_protocol_evaluation_review_protocol_protocol_id",
                table: "protocol_evaluation",
                column: "protocol_id",
                principalTable: "review_protocol",
                principalColumn: "protocol_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_protocol_version_review_protocol_protocol_id",
                table: "protocol_version",
                column: "protocol_id",
                principalTable: "review_protocol",
                principalColumn: "protocol_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_research_question_question_type_question_type_id",
                table: "research_question",
                column: "question_type_id",
                principalTable: "question_type",
                principalColumn: "question_type_id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_review_need_systematic_review_projects_project_id",
                table: "review_need",
                column: "project_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_review_objective_systematic_review_projects_project_id",
                table: "review_objective",
                column: "project_id",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_search_source_review_protocol_protocol_id",
                table: "search_source",
                column: "protocol_id",
                principalTable: "review_protocol",
                principalColumn: "protocol_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_search_string_search_strategy_strategy_id",
                table: "search_string",
                column: "strategy_id",
                principalTable: "search_strategy",
                principalColumn: "strategy_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_search_string_term_search_string_search_string_id",
                table: "search_string_term",
                column: "search_string_id",
                principalTable: "search_string",
                principalColumn: "search_string_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_search_string_term_search_term_term_id",
                table: "search_string_term",
                column: "term_id",
                principalTable: "search_term",
                principalColumn: "term_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_study_selection_criteria_review_protocol_protocol_id",
                table: "study_selection_criteria",
                column: "protocol_id",
                principalTable: "review_protocol",
                principalColumn: "protocol_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bibliographic_database_search_source_source_id",
                table: "bibliographic_database");

            migrationBuilder.DropForeignKey(
                name: "FK_commissioning_document_systematic_review_projects_project_id",
                table: "commissioning_document");

            migrationBuilder.DropForeignKey(
                name: "FK_conference_proceeding_search_source_source_id",
                table: "conference_proceeding");

            migrationBuilder.DropForeignKey(
                name: "FK_digital_library_search_source_source_id",
                table: "digital_library");

            migrationBuilder.DropForeignKey(
                name: "FK_exclusion_criterion_study_selection_criteria_criteria_id",
                table: "exclusion_criterion");

            migrationBuilder.DropForeignKey(
                name: "FK_inclusion_criterion_study_selection_criteria_criteria_id",
                table: "inclusion_criterion");

            migrationBuilder.DropForeignKey(
                name: "FK_journal_search_source_source_id",
                table: "journal");

            migrationBuilder.DropForeignKey(
                name: "FK_protocol_evaluation_protocol_reviewer_ProtocolReviewerId",
                table: "protocol_evaluation");

            migrationBuilder.DropForeignKey(
                name: "FK_protocol_evaluation_protocol_reviewer_reviewer_id",
                table: "protocol_evaluation");

            migrationBuilder.DropForeignKey(
                name: "FK_protocol_evaluation_review_protocol_protocol_id",
                table: "protocol_evaluation");

            migrationBuilder.DropForeignKey(
                name: "FK_protocol_version_review_protocol_protocol_id",
                table: "protocol_version");

            migrationBuilder.DropForeignKey(
                name: "FK_research_question_question_type_question_type_id",
                table: "research_question");

            migrationBuilder.DropForeignKey(
                name: "FK_review_need_systematic_review_projects_project_id",
                table: "review_need");

            migrationBuilder.DropForeignKey(
                name: "FK_review_objective_systematic_review_projects_project_id",
                table: "review_objective");

            migrationBuilder.DropForeignKey(
                name: "FK_search_source_review_protocol_protocol_id",
                table: "search_source");

            migrationBuilder.DropForeignKey(
                name: "FK_search_string_search_strategy_strategy_id",
                table: "search_string");

            migrationBuilder.DropForeignKey(
                name: "FK_search_string_term_search_string_search_string_id",
                table: "search_string_term");

            migrationBuilder.DropForeignKey(
                name: "FK_search_string_term_search_term_term_id",
                table: "search_string_term");

            migrationBuilder.DropForeignKey(
                name: "FK_study_selection_criteria_review_protocol_protocol_id",
                table: "study_selection_criteria");

            migrationBuilder.DropPrimaryKey(
                name: "PK_study_selection_criteria",
                table: "study_selection_criteria");

            migrationBuilder.DropPrimaryKey(
                name: "PK_search_term",
                table: "search_term");

            migrationBuilder.DropPrimaryKey(
                name: "PK_search_string",
                table: "search_string");

            migrationBuilder.DropPrimaryKey(
                name: "PK_search_source",
                table: "search_source");

            migrationBuilder.DropPrimaryKey(
                name: "PK_review_objective",
                table: "review_objective");

            migrationBuilder.DropPrimaryKey(
                name: "PK_review_need",
                table: "review_need");

            migrationBuilder.DropPrimaryKey(
                name: "PK_question_type",
                table: "question_type");

            migrationBuilder.DropPrimaryKey(
                name: "PK_protocol_version",
                table: "protocol_version");

            migrationBuilder.DropPrimaryKey(
                name: "PK_protocol_reviewer",
                table: "protocol_reviewer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_protocol_evaluation",
                table: "protocol_evaluation");

            migrationBuilder.DropIndex(
                name: "IX_protocol_evaluation_ProtocolReviewerId",
                table: "protocol_evaluation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_journal",
                table: "journal");

            migrationBuilder.DropPrimaryKey(
                name: "PK_inclusion_criterion",
                table: "inclusion_criterion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_exclusion_criterion",
                table: "exclusion_criterion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_digital_library",
                table: "digital_library");

            migrationBuilder.DropPrimaryKey(
                name: "PK_conference_proceeding",
                table: "conference_proceeding");

            migrationBuilder.DropPrimaryKey(
                name: "PK_commissioning_document",
                table: "commissioning_document");

            migrationBuilder.DropPrimaryKey(
                name: "PK_bibliographic_database",
                table: "bibliographic_database");

            migrationBuilder.DropColumn(
                name: "ProtocolReviewerId",
                table: "protocol_evaluation");

            migrationBuilder.RenameTable(
                name: "study_selection_criteria",
                newName: "StudySelectionCriterias");

            migrationBuilder.RenameTable(
                name: "search_term",
                newName: "SearchTerms");

            migrationBuilder.RenameTable(
                name: "search_string",
                newName: "SearchStrings");

            migrationBuilder.RenameTable(
                name: "search_source",
                newName: "SearchSources");

            migrationBuilder.RenameTable(
                name: "review_objective",
                newName: "ReviewObjectives");

            migrationBuilder.RenameTable(
                name: "review_need",
                newName: "ReviewNeeds");

            migrationBuilder.RenameTable(
                name: "question_type",
                newName: "QuestionTypes");

            migrationBuilder.RenameTable(
                name: "protocol_version",
                newName: "ProtocolVersions");

            migrationBuilder.RenameTable(
                name: "protocol_reviewer",
                newName: "ProtocolReviewers");

            migrationBuilder.RenameTable(
                name: "protocol_evaluation",
                newName: "ProtocolEvaluations");

            migrationBuilder.RenameTable(
                name: "journal",
                newName: "Journals");

            migrationBuilder.RenameTable(
                name: "inclusion_criterion",
                newName: "InclusionCriteria");

            migrationBuilder.RenameTable(
                name: "exclusion_criterion",
                newName: "ExclusionCriteria");

            migrationBuilder.RenameTable(
                name: "digital_library",
                newName: "DigitalLibraries");

            migrationBuilder.RenameTable(
                name: "conference_proceeding",
                newName: "ConferenceProceedings");

            migrationBuilder.RenameTable(
                name: "commissioning_document",
                newName: "CommissioningDocuments");

            migrationBuilder.RenameTable(
                name: "bibliographic_database",
                newName: "BibliographicDatabases");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "research_question",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "StudySelectionCriterias",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "protocol_id",
                table: "StudySelectionCriterias",
                newName: "ProtocolId");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "StudySelectionCriterias",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "StudySelectionCriterias",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "criteria_id",
                table: "StudySelectionCriterias",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_study_selection_criteria_protocol_id",
                table: "StudySelectionCriterias",
                newName: "IX_StudySelectionCriterias_ProtocolId");

            migrationBuilder.RenameColumn(
                name: "source",
                table: "SearchTerms",
                newName: "Source");

            migrationBuilder.RenameColumn(
                name: "keyword",
                table: "SearchTerms",
                newName: "Keyword");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "SearchTerms",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "SearchTerms",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "term_id",
                table: "SearchTerms",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "expression",
                table: "SearchStrings",
                newName: "Expression");

            migrationBuilder.RenameColumn(
                name: "strategy_id",
                table: "SearchStrings",
                newName: "StrategyId");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "SearchStrings",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "SearchStrings",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "search_string_id",
                table: "SearchStrings",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_search_string_strategy_id",
                table: "SearchStrings",
                newName: "IX_SearchStrings_StrategyId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "SearchSources",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "source_type",
                table: "SearchSources",
                newName: "SourceType");

            migrationBuilder.RenameColumn(
                name: "protocol_id",
                table: "SearchSources",
                newName: "ProtocolId");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "SearchSources",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "SearchSources",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "source_id",
                table: "SearchSources",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_search_source_protocol_id",
                table: "SearchSources",
                newName: "IX_SearchSources_ProtocolId");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "ReviewObjectives",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "objective_statement",
                table: "ReviewObjectives",
                newName: "ObjectiveStatement");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "ReviewObjectives",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ReviewObjectives",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "objective_id",
                table: "ReviewObjectives",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_review_objective_project_id",
                table: "ReviewObjectives",
                newName: "IX_ReviewObjectives_ProjectId");

            migrationBuilder.RenameColumn(
                name: "justification",
                table: "ReviewNeeds",
                newName: "Justification");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "ReviewNeeds",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "ReviewNeeds",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "ReviewNeeds",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "identified_by",
                table: "ReviewNeeds",
                newName: "IdentifiedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ReviewNeeds",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "need_id",
                table: "ReviewNeeds",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_review_need_project_id",
                table: "ReviewNeeds",
                newName: "IX_ReviewNeeds_ProjectId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "QuestionTypes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "QuestionTypes",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "QuestionTypes",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "QuestionTypes",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "question_type_id",
                table: "QuestionTypes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "version_number",
                table: "ProtocolVersions",
                newName: "VersionNumber");

            migrationBuilder.RenameColumn(
                name: "snapshot_data",
                table: "ProtocolVersions",
                newName: "SnapshotData");

            migrationBuilder.RenameColumn(
                name: "protocol_id",
                table: "ProtocolVersions",
                newName: "ProtocolId");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "ProtocolVersions",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ProtocolVersions",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "change_summary",
                table: "ProtocolVersions",
                newName: "ChangeSummary");

            migrationBuilder.RenameColumn(
                name: "version_id",
                table: "ProtocolVersions",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_protocol_version_protocol_id",
                table: "ProtocolVersions",
                newName: "IX_ProtocolVersions_ProtocolId");

            migrationBuilder.RenameColumn(
                name: "role",
                table: "ProtocolReviewers",
                newName: "Role");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "ProtocolReviewers",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "affiliation",
                table: "ProtocolReviewers",
                newName: "Affiliation");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "ProtocolReviewers",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ProtocolReviewers",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "reviewer_id",
                table: "ProtocolReviewers",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "comment",
                table: "ProtocolEvaluations",
                newName: "Comment");

            migrationBuilder.RenameColumn(
                name: "reviewer_id",
                table: "ProtocolEvaluations",
                newName: "ReviewerId");

            migrationBuilder.RenameColumn(
                name: "protocol_id",
                table: "ProtocolEvaluations",
                newName: "ProtocolId");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "ProtocolEvaluations",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "evaluation_result",
                table: "ProtocolEvaluations",
                newName: "EvaluationResult");

            migrationBuilder.RenameColumn(
                name: "evaluated_at",
                table: "ProtocolEvaluations",
                newName: "EvaluatedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ProtocolEvaluations",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "evaluation_id",
                table: "ProtocolEvaluations",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_protocol_evaluation_reviewer_id",
                table: "ProtocolEvaluations",
                newName: "IX_ProtocolEvaluations_ReviewerId");

            migrationBuilder.RenameIndex(
                name: "IX_protocol_evaluation_protocol_id",
                table: "ProtocolEvaluations",
                newName: "IX_ProtocolEvaluations_ProtocolId");

            migrationBuilder.RenameColumn(
                name: "source_id",
                table: "Journals",
                newName: "SourceId");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "Journals",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Journals",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "journal_id",
                table: "Journals",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_journal_source_id",
                table: "Journals",
                newName: "IX_Journals_SourceId");

            migrationBuilder.RenameColumn(
                name: "rule",
                table: "InclusionCriteria",
                newName: "Rule");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "InclusionCriteria",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "criteria_id",
                table: "InclusionCriteria",
                newName: "CriteriaId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "InclusionCriteria",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "inclusion_id",
                table: "InclusionCriteria",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_inclusion_criterion_criteria_id",
                table: "InclusionCriteria",
                newName: "IX_InclusionCriteria_CriteriaId");

            migrationBuilder.RenameColumn(
                name: "rule",
                table: "ExclusionCriteria",
                newName: "Rule");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "ExclusionCriteria",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "criteria_id",
                table: "ExclusionCriteria",
                newName: "CriteriaId");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ExclusionCriteria",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "exclusion_id",
                table: "ExclusionCriteria",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_exclusion_criterion_criteria_id",
                table: "ExclusionCriteria",
                newName: "IX_ExclusionCriteria_CriteriaId");

            migrationBuilder.RenameColumn(
                name: "source_id",
                table: "DigitalLibraries",
                newName: "SourceId");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "DigitalLibraries",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "DigitalLibraries",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "access_url",
                table: "DigitalLibraries",
                newName: "AccessUrl");

            migrationBuilder.RenameColumn(
                name: "library_id",
                table: "DigitalLibraries",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_digital_library_source_id",
                table: "DigitalLibraries",
                newName: "IX_DigitalLibraries_SourceId");

            migrationBuilder.RenameColumn(
                name: "source_id",
                table: "ConferenceProceedings",
                newName: "SourceId");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "ConferenceProceedings",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "ConferenceProceedings",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "conference_id",
                table: "ConferenceProceedings",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_conference_proceeding_source_id",
                table: "ConferenceProceedings",
                newName: "IX_ConferenceProceedings_SourceId");

            migrationBuilder.RenameColumn(
                name: "sponsor",
                table: "CommissioningDocuments",
                newName: "Sponsor");

            migrationBuilder.RenameColumn(
                name: "scope",
                table: "CommissioningDocuments",
                newName: "Scope");

            migrationBuilder.RenameColumn(
                name: "budget",
                table: "CommissioningDocuments",
                newName: "Budget");

            migrationBuilder.RenameColumn(
                name: "project_id",
                table: "CommissioningDocuments",
                newName: "ProjectId");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "CommissioningDocuments",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "document_url",
                table: "CommissioningDocuments",
                newName: "DocumentUrl");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "CommissioningDocuments",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "document_id",
                table: "CommissioningDocuments",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_commissioning_document_project_id",
                table: "CommissioningDocuments",
                newName: "IX_CommissioningDocuments_ProjectId");

            migrationBuilder.RenameColumn(
                name: "source_id",
                table: "BibliographicDatabases",
                newName: "SourceId");

            migrationBuilder.RenameColumn(
                name: "modified_at",
                table: "BibliographicDatabases",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "BibliographicDatabases",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "database_id",
                table: "BibliographicDatabases",
                newName: "Id");

            migrationBuilder.RenameIndex(
                name: "IX_bibliographic_database_source_id",
                table: "BibliographicDatabases",
                newName: "IX_BibliographicDatabases_SourceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StudySelectionCriterias",
                table: "StudySelectionCriterias",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SearchTerms",
                table: "SearchTerms",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SearchStrings",
                table: "SearchStrings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SearchSources",
                table: "SearchSources",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewObjectives",
                table: "ReviewObjectives",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ReviewNeeds",
                table: "ReviewNeeds",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuestionTypes",
                table: "QuestionTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProtocolVersions",
                table: "ProtocolVersions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProtocolReviewers",
                table: "ProtocolReviewers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProtocolEvaluations",
                table: "ProtocolEvaluations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Journals",
                table: "Journals",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InclusionCriteria",
                table: "InclusionCriteria",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ExclusionCriteria",
                table: "ExclusionCriteria",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DigitalLibraries",
                table: "DigitalLibraries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConferenceProceedings",
                table: "ConferenceProceedings",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CommissioningDocuments",
                table: "CommissioningDocuments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BibliographicDatabases",
                table: "BibliographicDatabases",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BibliographicDatabases_SearchSources_SourceId",
                table: "BibliographicDatabases",
                column: "SourceId",
                principalTable: "SearchSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CommissioningDocuments_systematic_review_projects_ProjectId",
                table: "CommissioningDocuments",
                column: "ProjectId",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConferenceProceedings_SearchSources_SourceId",
                table: "ConferenceProceedings",
                column: "SourceId",
                principalTable: "SearchSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DigitalLibraries_SearchSources_SourceId",
                table: "DigitalLibraries",
                column: "SourceId",
                principalTable: "SearchSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ExclusionCriteria_StudySelectionCriterias_CriteriaId",
                table: "ExclusionCriteria",
                column: "CriteriaId",
                principalTable: "StudySelectionCriterias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_InclusionCriteria_StudySelectionCriterias_CriteriaId",
                table: "InclusionCriteria",
                column: "CriteriaId",
                principalTable: "StudySelectionCriterias",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Journals_SearchSources_SourceId",
                table: "Journals",
                column: "SourceId",
                principalTable: "SearchSources",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProtocolEvaluations_ProtocolReviewers_ReviewerId",
                table: "ProtocolEvaluations",
                column: "ReviewerId",
                principalTable: "ProtocolReviewers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProtocolEvaluations_review_protocol_ProtocolId",
                table: "ProtocolEvaluations",
                column: "ProtocolId",
                principalTable: "review_protocol",
                principalColumn: "protocol_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProtocolVersions_review_protocol_ProtocolId",
                table: "ProtocolVersions",
                column: "ProtocolId",
                principalTable: "review_protocol",
                principalColumn: "protocol_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_research_question_QuestionTypes_question_type_id",
                table: "research_question",
                column: "question_type_id",
                principalTable: "QuestionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewNeeds_systematic_review_projects_ProjectId",
                table: "ReviewNeeds",
                column: "ProjectId",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewObjectives_systematic_review_projects_ProjectId",
                table: "ReviewObjectives",
                column: "ProjectId",
                principalTable: "systematic_review_projects",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_search_string_term_SearchStrings_search_string_id",
                table: "search_string_term",
                column: "search_string_id",
                principalTable: "SearchStrings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_search_string_term_SearchTerms_term_id",
                table: "search_string_term",
                column: "term_id",
                principalTable: "SearchTerms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SearchSources_review_protocol_ProtocolId",
                table: "SearchSources",
                column: "ProtocolId",
                principalTable: "review_protocol",
                principalColumn: "protocol_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SearchStrings_search_strategy_StrategyId",
                table: "SearchStrings",
                column: "StrategyId",
                principalTable: "search_strategy",
                principalColumn: "strategy_id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StudySelectionCriterias_review_protocol_ProtocolId",
                table: "StudySelectionCriterias",
                column: "ProtocolId",
                principalTable: "review_protocol",
                principalColumn: "protocol_id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
