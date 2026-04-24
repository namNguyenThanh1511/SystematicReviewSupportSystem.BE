using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.IdentificationProcessRepo;
using SRSS.IAM.Repositories.ImportBatchRepo;
using SRSS.IAM.Repositories.PaperRepo;
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
using SRSS.IAM.Repositories.MasterSearchSourceRepo;
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
using SRSS.IAM.Repositories.AuditLogRepo;

using SRSS.IAM.Repositories.FilterSettingRepo;
using SRSS.IAM.Repositories.ProjectPicocRepo;


namespace SRSS.IAM.Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _dbContext;
        private IDbContextTransaction? _currentTransaction;
        private IUserRepository? _users;
        private IAuditLogRepository? _auditLogs;
        private INotificationRepository? _notifications;
        private IProjectMemberInvitationRepository? _projectMemberInvitations;
        private ISystematicReviewProjectRepository? _systematicReviewProjects;
        private IProjectPicocRepository? _projectPicocs;
        // Core Governance
        private IReviewNeedRepository? _reviewNeeds;
        private ICommissioningDocumentRepository? _commissioningDocuments;
        private IReviewObjectiveRepository? _reviewObjectives;
        private IQuestionTypeRepository? _questionTypes;
        private IReviewProcessRepository? _reviewProcesses;
        private IFilterSettingRepository? _filterSettings;
        private IIdentificationProcessRepository? _identificationProcesses;
        private IPaperRepository? _papers;
        private IImportBatchRepository? _importBatches;
        private IPrismaReportRepository? _prismaReports;
        private IChecklistTemplateRepository? _checklistTemplates;
        private IChecklistItemTemplateRepository? _checklistItemTemplates;
        private IReviewChecklistRepository? _reviewChecklists;
        private IChecklistItemResponseRepository? _checklistItemResponses;
        private IDeduplicationResultRepository? _deduplicationResults;
        private IScreeningResolutionRepository? _screeningResolutions;
        private IStudySelectionProcessRepository? _studySelectionProcesses;
        private IScreeningDecisionRepository? _screeningDecisions;
        private IIdentificationProcessPaperRepository? _identificationProcessPapers;
        private IPaperAssignmentRepository? _paperAssignments;
        private ITitleAbstractScreeningRepository? _titleAbstractScreenings;
        private IPaperPdfRepository? _paperPdfs;
        private IPaperSourceMetadataRepository? _paperSourceMetadatas;
        private IGrobidHeaderResultRepository? _grobidHeaderResults;
        private ICandidatePaperRepository? _candidatePapers;
        private IPaperCitationRepository? _paperCitations;
        private IPaperEmbeddingRepository? _paperEmbeddings;
        private IStudySelectionProcessPaperRepository? _studySelectionProcessPapers;
        private IStudySelectionAIResultRepository? _studySelectionAIResults;
        private IPaperFullTextRepository? _paperFullTexts;
        private IPaperFullTextParsedSectionRepository? _paperFullTextParsedSections;
        private IPaperFullTextParsedParagraphRepository? _paperFullTextParsedParagraphs;
        private IPaperFullTextChunkRepository? _paperFullTextChunks;
        private IPaperFullTextChunkEmbeddingRepository? _paperFullTextChunkEmbeddings;
        private IMasterSearchSourceRepository? _masterSearchSources;


        // Research Question
        private IResearchQuestionRepository? _researchQuestions;
        private IPicocElementRepository? _picocElements;
        private IPopulationRepository? _populations;
        private IInterventionRepository? _interventions;
        private IComparisonRepository? _comparisons;
        private IOutcomeRepository? _outcomes;
        private IContextRepository? _contexts;

        // Search Strategy
        private ISearchSourceRepository? _searchSources;
        private IStudySelectionCriteriaRepository? _selectionCriterias;
        private IInclusionCriterionRepository? _inclusionCriteria;
        private IExclusionCriterionRepository? _exclusionCriteria;
        private IStudySelectionCriteriaAIResponseRepository? _studySelectionCriteriaAIResponses;
        private IQualityAssessmentStrategyRepository? _qualityStrategies;
        private IQualityChecklistRepository? _qualityChecklists;
        private IQualityCriterionRepository? _qualityCriteria;
        //private IDataExtractionStrategyRepository? _extractionStrategies;
        //private IDataExtractionFormRepository? _extractionForms;
        //private IDataItemDefinitionRepository? _dataItems;
        // Data Extraction

        // Quality Assessment
        private IQualityAssessmentProcessRepository? _qualityAssessmentProcesses;
        private IQualityAssessmentAssignmentRepository? _qualityAssessmentAssignments;
        private IQualityAssessmentDecisionRepository? _qualityAssessmentDecisions;
        private IQualityAssessmentResolutionRepository? _qualityAssessmentResolutions;

        private IExtractionTemplateRepository? _extractionTemplates;
        private IExtractionSectionRepository? _extractionSections;
        private IExtractionFieldRepository? _extractionFields;
        private IExtractionMatrixColumnRepository? _extractionMatrixColumns;
        private IFieldOptionRepository? _fieldOptions;
        private IDataExtractionProcessRepository? _dataExtractionProcesses;
        private IExtractionPaperTaskRepository? _extractionPaperTasks;
        private IExtractedDataValueRepository? _extractedDataValues;
        private IExtractionCommentRepository? _extractionComments;
        private IExtractedDataAuditLogRepository? _extractedDataAuditLogs;
        private IExclusionReasonLibraryRepository? _exclusionReasonLibraries;
        private IStudySelectionExclusionReasonRepository? _studySelectionExclusionReasons;
        private IStuSeExclusionCodeRepository? _stuSeExclusionCodes;

        // Study Selection Checklist
        private IStudySelectionChecklistTemplateRepository? _studySelectionChecklistTemplates;
        private IStudySelectionChecklistTemplateSectionRepository? _studySelectionChecklistTemplateSections;
        private IStudySelectionChecklistTemplateItemRepository? _studySelectionChecklistTemplateItems;
        private IStudySelectionChecklistSubmissionRepository? _studySelectionChecklistSubmissions;
        private IStudySelectionChecklistSubmissionSectionAnswerRepository? _studySelectionChecklistSubmissionSectionAnswers;
        private IStudySelectionChecklistSubmissionItemAnswerRepository? _studySelectionChecklistSubmissionItemAnswers;

        private IDataSynthesisStrategyRepository? _synthesisStrategies;
        private ISynthesisProcessRepository? _synthesisProcesses;
        private ISynthesisThemeRepository? _synthesisThemes;
        private IThemeEvidenceRepository? _themeEvidences;
        private IResearchQuestionFindingRepository? _researchQuestionFindings;

        public UnitOfWork(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction != null) return;
            _currentTransaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null) return;

            try
            {
                await SaveChangesAsync(cancellationToken);
                await _currentTransaction.CommitAsync(cancellationToken);
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_currentTransaction == null) return;

            try
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var date = DateTime.UtcNow;
            foreach (var entry in _dbContext.ChangeTracker.Entries<IBaseEntity>())
            {
                if (entry.State == EntityState.Added)
                    entry.Entity.CreatedAt = date;

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified || entry.HasChangedOwnedEntities())
                    entry.Entity.ModifiedAt = date;
            }

            return await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public IUserRepository Users => _users ??= new UserRepository(_dbContext);
        public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_dbContext);
        public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_dbContext);

        // Core Governance
        public IReviewNeedRepository ReviewNeeds => _reviewNeeds ??= new ReviewNeedRepository(_dbContext);
        public ICommissioningDocumentRepository CommissioningDocuments => _commissioningDocuments ??= new CommissioningDocumentRepository(_dbContext);
        public IReviewObjectiveRepository ReviewObjectives => _reviewObjectives ??= new ReviewObjectiveRepository(_dbContext);
        public IQuestionTypeRepository QuestionTypes => _questionTypes ??= new QuestionTypeRepository(_dbContext);



        // Research Question
        public IResearchQuestionRepository ResearchQuestions => _researchQuestions ??= new ResearchQuestionRepository(_dbContext);
        public IPicocElementRepository PicocElements => _picocElements ??= new PicocElementRepository(_dbContext);
        public IPopulationRepository Populations => _populations ??= new PopulationRepository(_dbContext);
        public IInterventionRepository Interventions => _interventions ??= new InterventionRepository(_dbContext);
        public IComparisonRepository Comparisons => _comparisons ??= new ComparisonRepository(_dbContext);
        public IOutcomeRepository Outcomes => _outcomes ??= new OutcomeRepository(_dbContext);
        public IContextRepository Contexts => _contexts ??= new ContextRepository(_dbContext);

        // Search Strategy
        public ISearchSourceRepository SearchSources => _searchSources ??= new SearchSourceRepository(_dbContext);

        public IStudySelectionCriteriaRepository SelectionCriterias =>
            _selectionCriterias ??= new StudySelectionCriteriaRepository(_dbContext);

        public IInclusionCriterionRepository InclusionCriteria =>
            _inclusionCriteria ??= new InclusionCriterionRepository(_dbContext);

        public IExclusionCriterionRepository ExclusionCriteria =>
            _exclusionCriteria ??= new ExclusionCriterionRepository(_dbContext);

        public IStudySelectionCriteriaAIResponseRepository StudySelectionCriteriaAIResponses =>
            _studySelectionCriteriaAIResponses ??= new StudySelectionCriteriaAIResponseRepository(_dbContext);



        public IQualityAssessmentStrategyRepository QualityStrategies =>
            _qualityStrategies ??= new QualityAssessmentStrategyRepository(_dbContext);

        public IQualityChecklistRepository QualityChecklists =>
            _qualityChecklists ??= new QualityChecklistRepository(_dbContext);

        public IQualityCriterionRepository QualityCriteria =>
            _qualityCriteria ??= new QualityCriterionRepository(_dbContext);

        // Data Extraction
        public IExtractionTemplateRepository ExtractionTemplates =>
            _extractionTemplates ??= new ExtractionTemplateRepository(_dbContext);

        public IExtractionSectionRepository ExtractionSections =>
            _extractionSections ??= new ExtractionSectionRepository(_dbContext);

        public IExtractionFieldRepository ExtractionFields =>
            _extractionFields ??= new ExtractionFieldRepository(_dbContext);

        public IExtractionMatrixColumnRepository ExtractionMatrixColumns =>
            _extractionMatrixColumns ??= new ExtractionMatrixColumnRepository(_dbContext);

        public IFieldOptionRepository FieldOptions =>
            _fieldOptions ??= new FieldOptionRepository(_dbContext);

        public IDataExtractionProcessRepository DataExtractionProcesses =>
            _dataExtractionProcesses ??= new DataExtractionProcessRepository(_dbContext);

        public IExtractionPaperTaskRepository ExtractionPaperTasks =>
            _extractionPaperTasks ??= new ExtractionPaperTaskRepository(_dbContext);

        public IExtractedDataValueRepository ExtractedDataValues =>
            _extractedDataValues ??= new ExtractedDataValueRepository(_dbContext);

        public IExtractionCommentRepository ExtractionComments =>
            _extractionComments ??= new ExtractionCommentRepository(_dbContext);

        public IExtractedDataAuditLogRepository ExtractedDataAuditLogs =>
            _extractedDataAuditLogs ??= new ExtractedDataAuditLogRepository(_dbContext);


        public IDataSynthesisStrategyRepository SynthesisStrategies =>
            _synthesisStrategies ??= new DataSynthesisStrategyRepository(_dbContext);

        public ISynthesisProcessRepository SynthesisProcesses =>
            _synthesisProcesses ??= new SynthesisProcessRepository(_dbContext);

        public ISynthesisThemeRepository SynthesisThemes =>
            _synthesisThemes ??= new SynthesisThemeRepository(_dbContext);

        public IThemeEvidenceRepository ThemeEvidences =>
            _themeEvidences ??= new ThemeEvidenceRepository(_dbContext);

        public IResearchQuestionFindingRepository ResearchQuestionFindings =>
            _researchQuestionFindings ??= new ResearchQuestionFindingRepository(_dbContext);

        public ISystematicReviewProjectRepository SystematicReviewProjects
            => _systematicReviewProjects ??= new SystematicReviewProjectRepository(_dbContext);

        public IProjectPicocRepository ProjectPicocs
            => _projectPicocs ??= new ProjectPicocRepository(_dbContext);

        public IReviewProcessRepository ReviewProcesses
            => _reviewProcesses ??= new ReviewProcessRepository(_dbContext);

        public IFilterSettingRepository FilterSettings
            => _filterSettings ??= new FilterSettingRepository(_dbContext);

        public IIdentificationProcessRepository IdentificationProcesses
            => _identificationProcesses ??= new IdentificationProcessRepository(_dbContext);


        public IPaperRepository Papers
            => _papers ??= new PaperRepository(_dbContext);

        public IImportBatchRepository ImportBatches
            => _importBatches ??= new ImportBatchRepository(_dbContext);

        public IPrismaReportRepository PrismaReports
            => _prismaReports ??= new PrismaReportRepository(_dbContext);

        public IChecklistTemplateRepository ChecklistTemplates
            => _checklistTemplates ??= new ChecklistTemplateRepository(_dbContext);

        public IChecklistItemTemplateRepository ChecklistItemTemplates
            => _checklistItemTemplates ??= new ChecklistItemTemplateRepository(_dbContext);

        public IReviewChecklistRepository ReviewChecklists
            => _reviewChecklists ??= new ReviewChecklistRepository(_dbContext);

        public IChecklistItemResponseRepository ChecklistItemResponses
            => _checklistItemResponses ??= new ChecklistItemResponseRepository(_dbContext);

        public IDeduplicationResultRepository DeduplicationResults
            => _deduplicationResults ??= new DeduplicationResultRepository(_dbContext);

        public IScreeningResolutionRepository ScreeningResolutions
            => _screeningResolutions ??= new ScreeningResolutionRepository(_dbContext);

        public IStudySelectionProcessRepository StudySelectionProcesses
            => _studySelectionProcesses ??= new StudySelectionProcessRepository(_dbContext);

        public IScreeningDecisionRepository ScreeningDecisions
            => _screeningDecisions ??= new ScreeningDecisionRepository(_dbContext);

        public IProjectMemberInvitationRepository ProjectMemberInvitations
            => _projectMemberInvitations ??= new ProjectMemberInvitationRepository(_dbContext);

        public IIdentificationProcessPaperRepository IdentificationProcessPapers
            => _identificationProcessPapers ??= new IdentificationProcessPaperRepository(_dbContext);

        public IPaperAssignmentRepository PaperAssignments
            => _paperAssignments ??= new PaperAssignmentRepository(_dbContext);
        public ITitleAbstractScreeningRepository TitleAbstractScreenings
            => _titleAbstractScreenings ??= new TitleAbstractScreeningRepository(_dbContext);
        public IPaperPdfRepository PaperPdfs
            => _paperPdfs ??= new PaperPdfRepository(_dbContext);
        public IPaperSourceMetadataRepository PaperSourceMetadatas
            => _paperSourceMetadatas ??= new PaperSourceMetadataRepository(_dbContext);
        public IGrobidHeaderResultRepository GrobidHeaderResults
            => _grobidHeaderResults ??= new GrobidHeaderResultRepository(_dbContext);
        public ICandidatePaperRepository CandidatePapers
            => _candidatePapers ??= new CandidatePaperRepository(_dbContext);
        public IPaperCitationRepository PaperCitations
            => _paperCitations ??= new PaperCitationRepository(_dbContext);
        public IPaperEmbeddingRepository PaperEmbeddings
            => _paperEmbeddings ??= new PaperEmbeddingRepository(_dbContext);
        public IStudySelectionProcessPaperRepository StudySelectionProcessPapers
            => _studySelectionProcessPapers ??= new StudySelectionProcessPaperRepository(_dbContext);
        public IStudySelectionAIResultRepository StudySelectionAIResults
            => _studySelectionAIResults ??= new StudySelectionAIResultRepository(_dbContext);
        public IPaperFullTextRepository PaperFullTexts
            => _paperFullTexts ??= new PaperFullTextRepository(_dbContext);
        public IPaperFullTextParsedSectionRepository PaperFullTextParsedSections
            => _paperFullTextParsedSections ??= new PaperFullTextParsedSectionRepository(_dbContext);
        public IPaperFullTextParsedParagraphRepository PaperFullTextParsedParagraphs
            => _paperFullTextParsedParagraphs ??= new PaperFullTextParsedParagraphRepository(_dbContext);
        public IPaperFullTextChunkRepository PaperFullTextChunks
            => _paperFullTextChunks ??= new PaperFullTextChunkRepository(_dbContext);
        public IPaperFullTextChunkEmbeddingRepository PaperFullTextChunkEmbeddings
            => _paperFullTextChunkEmbeddings ??= new PaperFullTextChunkEmbeddingRepository(_dbContext);
        public IMasterSearchSourceRepository MasterSearchSources
            => _masterSearchSources ??= new MasterSearchSourceRepository(_dbContext);

        // Quality Assessment
        public IQualityAssessmentProcessRepository QualityAssessmentProcesses => _qualityAssessmentProcesses ??= new QualityAssessmentProcessRepository(_dbContext);
        public IQualityAssessmentAssignmentRepository QualityAssessmentAssignments => _qualityAssessmentAssignments ??= new QualityAssessmentAssignmentRepository(_dbContext);
        public IQualityAssessmentDecisionRepository QualityAssessmentDecisions => _qualityAssessmentDecisions ??= new QualityAssessmentDecisionRepository(_dbContext);
        public IQualityAssessmentResolutionRepository QualityAssessmentResolutions => _qualityAssessmentResolutions ??= new QualityAssessmentResolutionRepository(_dbContext);
        public IExclusionReasonLibraryRepository ExclusionReasonLibraries => _exclusionReasonLibraries ??= new ExclusionReasonLibraryRepository(_dbContext);
        public IStudySelectionExclusionReasonRepository StudySelectionExclusionReasons => _studySelectionExclusionReasons ??= new StudySelectionExclusionReasonRepository(_dbContext);
        public IStuSeExclusionCodeRepository StuSeExclusionCodes => _stuSeExclusionCodes ??= new StuSeExclusionCodeRepository(_dbContext);

        // Study Selection Checklist
        public IStudySelectionChecklistTemplateRepository StudySelectionChecklistTemplates => _studySelectionChecklistTemplates ??= new StudySelectionChecklistTemplateRepository(_dbContext);
        public IStudySelectionChecklistTemplateSectionRepository StudySelectionChecklistTemplateSections => _studySelectionChecklistTemplateSections ??= new StudySelectionChecklistTemplateSectionRepository(_dbContext);
        public IStudySelectionChecklistTemplateItemRepository StudySelectionChecklistTemplateItems => _studySelectionChecklistTemplateItems ??= new StudySelectionChecklistTemplateItemRepository(_dbContext);
        public IStudySelectionChecklistSubmissionRepository StudySelectionChecklistSubmissions => _studySelectionChecklistSubmissions ??= new StudySelectionChecklistSubmissionRepository(_dbContext);
        public IStudySelectionChecklistSubmissionSectionAnswerRepository StudySelectionChecklistSubmissionSectionAnswers => _studySelectionChecklistSubmissionSectionAnswers ??= new StudySelectionChecklistSubmissionSectionAnswerRepository(_dbContext);
        public IStudySelectionChecklistSubmissionItemAnswerRepository StudySelectionChecklistSubmissionItemAnswers => _studySelectionChecklistSubmissionItemAnswers ??= new StudySelectionChecklistSubmissionItemAnswerRepository(_dbContext);

        public void ClearTracker() => _dbContext.ChangeTracker.Clear();

        public void Dispose() => _dbContext.Dispose();
    }

    public static class Extensions
    {
        public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
            entry.References.Any(r =>
                r.TargetEntry != null &&
                r.TargetEntry.Metadata.IsOwned() &&
                (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
    }
}
