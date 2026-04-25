using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Repositories;
using SRSS.IAM.Repositories.AuditLogRepo;
using SRSS.IAM.Repositories.CoreGovernRepo;
using SRSS.IAM.Repositories.DataExtractionRepo;
using SRSS.IAM.Repositories.QualityRepo;
using SRSS.IAM.Repositories.ResearchQuestionRepo;
using SRSS.IAM.Repositories.SearchStrategyRepo;
using SRSS.IAM.Repositories.StudySelectionCriteriaRepo;
using SRSS.IAM.Repositories.SynthesisRepo;
using SRSS.IAM.Repositories.SynthesisExecutionRepo;
using SRSS.IAM.Repositories.UserRepo;
using SRSS.IAM.Repositories.NotificationRepo;
using SRSS.IAM.Repositories.IdentificationProcessRepo;
using SRSS.IAM.Repositories.MasterSearchSourceRepo;
using SRSS.IAM.Repositories.PaperRepo;
using SRSS.IAM.Repositories.ImportBatchRepo;
using SRSS.IAM.Repositories.SystematicReviewProjectRepo;
using SRSS.IAM.Repositories.ReviewProcessRepo;
using SRSS.IAM.Repositories.PrismaReportRepo;
using SRSS.IAM.Repositories.DeduplicationResultRepo;
using SRSS.IAM.Repositories.ScreeningResolutionRepo;
using SRSS.IAM.Repositories.StudySelectionProcessRepo;
using SRSS.IAM.Repositories.ScreeningDecisionRepo;
using SRSS.IAM.Repositories.ProjectMemberInvitationRepo;
using SRSS.IAM.Repositories.IdentificationProcessPaperRepo;
using SRSS.IAM.Repositories.PaperAssignmentRepo;
using SRSS.IAM.Repositories.TitleAbstractScreeningRepo;
using SRSS.IAM.Repositories.PaperPdfRepo;
using SRSS.IAM.Repositories.PaperSourceMetadataRepo;
using SRSS.IAM.Repositories.GrobidHeaderResultRepo;
using SRSS.IAM.Repositories.CandidatePaperRepo;
using SRSS.IAM.Repositories.PaperCitationRepo;
using SRSS.IAM.Repositories.PaperEmbeddingRepo;
using SRSS.IAM.Repositories.StudySelectionProcessPaperRepo;
using SRSS.IAM.Repositories.StudySelectionAIResultRepo;
using SRSS.IAM.Repositories.PaperFullTextRepo;
using SRSS.IAM.Repositories.ExclusionReasonLibraryRepo;
using SRSS.IAM.Repositories.StudySelectionExclusionReasonRepo;
using SRSS.IAM.Repositories.StuSeExclusionCodeRepo;
using SRSS.IAM.Repositories.ChecklistRepo;
using SRSS.IAM.Repositories.StudySelectionChecklistRepo;
using SRSS.IAM.Repositories.PaperFullTextParsedSectionRepo;
using SRSS.IAM.Repositories.PaperFullTextParsedParagraphRepo;
using SRSS.IAM.Repositories.PaperFullTextChunkRepo;
using SRSS.IAM.Repositories.PaperFullTextChunkEmbeddingRepo;
using SRSS.IAM.Repositories.FilterSettingRepo;
using SRSS.IAM.Repositories.ProjectPicocRepo;


namespace SRSS.IAM.Repositories.UnitOfWork
{
    // Service-specific interface extends base
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
        IUserRepository Users { get; }
        INotificationRepository Notifications { get; }
        IProjectMemberInvitationRepository ProjectMemberInvitations { get; }

        // Core Governance
        IReviewNeedRepository ReviewNeeds { get; }
        ICommissioningDocumentRepository CommissioningDocuments { get; }
        IReviewObjectiveRepository ReviewObjectives { get; }
        IQuestionTypeRepository QuestionTypes { get; }



        // Research Question
        IResearchQuestionRepository ResearchQuestions { get; }
        IPicocElementRepository PicocElements { get; }
        IPopulationRepository Populations { get; }
        IInterventionRepository Interventions { get; }
        IComparisonRepository Comparisons { get; }
        IOutcomeRepository Outcomes { get; }
        IContextRepository Contexts { get; }

        // Search Strategy
        ISearchSourceRepository SearchSources { get; }

        // Selection Criteria
        IStudySelectionCriteriaRepository SelectionCriterias { get; }
        IInclusionCriterionRepository InclusionCriteria { get; }
        IExclusionCriterionRepository ExclusionCriteria { get; }
        IStudySelectionCriteriaAIResponseRepository StudySelectionCriteriaAIResponses { get; }

        // Quality Assessment
        IQualityAssessmentStrategyRepository QualityStrategies { get; }
        IQualityChecklistRepository QualityChecklists { get; }
        IQualityCriterionRepository QualityCriteria { get; }

        // Data Extraction
        //IDataExtractionStrategyRepository ExtractionStrategies { get; }
        //IDataExtractionFormRepository ExtractionForms { get; }
        //IDataItemDefinitionRepository DataItems { get; }
        IExtractionTemplateRepository ExtractionTemplates { get; }
        IExtractionSectionRepository ExtractionSections { get; }
        IExtractionFieldRepository ExtractionFields { get; }
        IExtractionMatrixColumnRepository ExtractionMatrixColumns { get; }
        IFieldOptionRepository FieldOptions { get; }
        IDataExtractionProcessRepository DataExtractionProcesses { get; }
        IExtractionPaperTaskRepository ExtractionPaperTasks { get; }
        IExtractedDataValueRepository ExtractedDataValues { get; }
        IExtractionCommentRepository ExtractionComments { get; }
        IExtractedDataAuditLogRepository ExtractedDataAuditLogs { get; }

        // Synthesis
        ISynthesisProcessRepository SynthesisProcesses { get; }
        ISynthesisThemeRepository SynthesisThemes { get; }
        IThemeEvidenceRepository ThemeEvidences { get; }
        IResearchQuestionFindingRepository ResearchQuestionFindings { get; }
        IDataSynthesisStrategyRepository SynthesisStrategies { get; }
        ISystematicReviewProjectRepository SystematicReviewProjects { get; }
        IProjectPicocRepository ProjectPicocs { get; }
        IReviewProcessRepository ReviewProcesses { get; }

        IFilterSettingRepository FilterSettings { get; }
        IIdentificationProcessRepository IdentificationProcesses { get; }
        IPaperRepository Papers { get; }
        IImportBatchRepository ImportBatches { get; }
        IPrismaReportRepository PrismaReports { get; }
        IChecklistTemplateRepository ChecklistTemplates { get; }
        IChecklistItemTemplateRepository ChecklistItemTemplates { get; }
        IReviewChecklistRepository ReviewChecklists { get; }
        IChecklistItemResponseRepository ChecklistItemResponses { get; }
        IDeduplicationResultRepository DeduplicationResults { get; }
        IScreeningResolutionRepository ScreeningResolutions { get; }
        IStudySelectionProcessRepository StudySelectionProcesses { get; }
        IScreeningDecisionRepository ScreeningDecisions { get; }
        IIdentificationProcessPaperRepository IdentificationProcessPapers { get; }
        IPaperAssignmentRepository PaperAssignments { get; }
        ITitleAbstractScreeningRepository TitleAbstractScreenings { get; }
        IPaperPdfRepository PaperPdfs { get; }
        IPaperSourceMetadataRepository PaperSourceMetadatas { get; }
        IGrobidHeaderResultRepository GrobidHeaderResults { get; }
        ICandidatePaperRepository CandidatePapers { get; }
        IPaperCitationRepository PaperCitations { get; }
        IPaperEmbeddingRepository PaperEmbeddings { get; }
        IStudySelectionProcessPaperRepository StudySelectionProcessPapers { get; }
        IStudySelectionAIResultRepository StudySelectionAIResults { get; }
        IPaperFullTextRepository PaperFullTexts { get; }
        IPaperFullTextParsedSectionRepository PaperFullTextParsedSections { get; }
        IPaperFullTextParsedParagraphRepository PaperFullTextParsedParagraphs { get; }
        IPaperFullTextChunkRepository PaperFullTextChunks { get; }
        IPaperFullTextChunkEmbeddingRepository PaperFullTextChunkEmbeddings { get; }
        IMasterSearchSourceRepository MasterSearchSources { get; }

        // Quality Assessment
        IQualityAssessmentProcessRepository QualityAssessmentProcesses { get; }
        IQualityAssessmentAssignmentRepository QualityAssessmentAssignments { get; }
        IQualityAssessmentDecisionRepository QualityAssessmentDecisions { get; }
        IQualityAssessmentResolutionRepository QualityAssessmentResolutions { get; }
        IExclusionReasonLibraryRepository ExclusionReasonLibraries { get; }
        IStudySelectionExclusionReasonRepository StudySelectionExclusionReasons { get; }
        IStuSeExclusionCodeRepository StuSeExclusionCodes { get; }

        // Study Selection Checklist
        IStudySelectionChecklistTemplateRepository StudySelectionChecklistTemplates { get; }
        IStudySelectionChecklistTemplateSectionRepository StudySelectionChecklistTemplateSections { get; }
        IStudySelectionChecklistTemplateItemRepository StudySelectionChecklistTemplateItems { get; }
        IStudySelectionChecklistSubmissionRepository StudySelectionChecklistSubmissions { get; }
        IStudySelectionChecklistSubmissionSectionAnswerRepository StudySelectionChecklistSubmissionSectionAnswers { get; }
        IStudySelectionChecklistSubmissionItemAnswerRepository StudySelectionChecklistSubmissionItemAnswers { get; }
        IAuditLogRepository AuditLogs { get; }
        void ClearTracker();
    }
}
