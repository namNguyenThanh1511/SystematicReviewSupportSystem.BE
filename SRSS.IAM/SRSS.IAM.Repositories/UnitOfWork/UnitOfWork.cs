using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.IdentificationProcessRepo;
using SRSS.IAM.Repositories.ImportBatchRepo;
using SRSS.IAM.Repositories.PaperRepo;
using SRSS.IAM.Repositories.SearchExecutionRepo;
using SRSS.IAM.Repositories.DataExtractionRepo;
using SRSS.IAM.Repositories.ProtocolRepo;
using SRSS.IAM.Repositories.QualityRepo;
using SRSS.IAM.Repositories.ResearchQuestionRepo;
using SRSS.IAM.Repositories.SearchStrategyRepo;
using SRSS.IAM.Repositories.StudySelectionCriteriaRepo;
using SRSS.IAM.Repositories.SynthesisRepo;
using SRSS.IAM.Repositories.UserRepo;
using SRSS.IAM.Repositories.SystematicReviewProjectRepo;
using SRSS.IAM.Repositories.ReviewProcessRepo;
using SRSS.IAM.Repositories.PrismaReportRepo;
using SRSS.IAM.Repositories.DeduplicationResultRepo;
using SRSS.IAM.Repositories.ScreeningResolutionRepo;

namespace SRSS.IAM.Repositories.UnitOfWork
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly AppDbContext _dbContext;
		private IDbContextTransaction? _currentTransaction;
		private IUserRepository _users;
		private ISystematicReviewProjectRepository _systematicReviewProjects;
		private IReviewProcessRepository _reviewProcesses;
		private IIdentificationProcessRepository _identificationProcesses;
		private ISearchExecutionRepository _searchExecutions;
		private IPaperRepository _papers;
		private IImportBatchRepository _importBatches;
		private IPrismaReportRepository _prismaReports;
		private IDeduplicationResultRepository _deduplicationResults;
		private IScreeningResolutionRepository _screeningResolutions;
		// Protocol
		private IReviewProtocolRepository? _protocols;
		private IProtocolVersionRepository? _protocolVersions;
		private IProtocolEvaluationRepository? _protocolEvaluations;

		// Research Question
		private IResearchQuestionRepository? _researchQuestions;
		private IPicocElementRepository? _picocElements;
		private IPopulationRepository? _populations;
		private IInterventionRepository? _interventions;
		private IComparisonRepository? _comparisons;
		private IOutcomeRepository? _outcomes;
		private IContextRepository? _contexts;

		// Search Strategy
		private ISearchStrategyRepository? _searchStrategies;
		private ISearchStringRepository? _searchStrings;
		private ISearchTermRepository? _searchTerms;
		private ISearchStringTermRepository? _searchStringTerms;
		private ISearchSourceRepository? _searchSources;
		private IStudySelectionCriteriaRepository? _selectionCriterias;
		private IInclusionCriterionRepository? _inclusionCriteria;
		private IExclusionCriterionRepository? _exclusionCriteria;
		private IStudySelectionProcedureRepository? _selectionProcedures;
		private IQualityAssessmentStrategyRepository? _qualityStrategies;
		private IQualityChecklistRepository? _qualityChecklists;
		private IQualityCriterionRepository? _qualityCriteria;
		private IDataExtractionStrategyRepository? _extractionStrategies;
		private IDataExtractionFormRepository? _extractionForms;
		private IDataItemDefinitionRepository? _dataItems;
		private IDataSynthesisStrategyRepository? _synthesisStrategies;
		private IDisseminationStrategyRepository? _disseminationStrategies;
		private IProjectTimetableRepository? _timetables;

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

		// Protocol
		public IReviewProtocolRepository Protocols => _protocols ??= new ReviewProtocolRepository(_dbContext);
		public IProtocolVersionRepository ProtocolVersions => _protocolVersions ??= new ProtocolVersionRepository(_dbContext);
		public IProtocolEvaluationRepository ProtocolEvaluations => _protocolEvaluations ??= new ProtocolEvaluationRepository(_dbContext);

		// Research Question
		public IResearchQuestionRepository ResearchQuestions => _researchQuestions ??= new ResearchQuestionRepository(_dbContext);
		public IPicocElementRepository PicocElements => _picocElements ??= new PicocElementRepository(_dbContext);
		public IPopulationRepository Populations => _populations ??= new PopulationRepository(_dbContext);
		public IInterventionRepository Interventions => _interventions ??= new InterventionRepository(_dbContext);
		public IComparisonRepository Comparisons => _comparisons ??= new ComparisonRepository(_dbContext);
		public IOutcomeRepository Outcomes => _outcomes ??= new OutcomeRepository(_dbContext);
		public IContextRepository Contexts => _contexts ??= new ContextRepository(_dbContext);

		// Search Strategy
		public ISearchStrategyRepository SearchStrategies => _searchStrategies ??= new SearchStrategyRepository(_dbContext);
		public ISearchStringRepository SearchStrings => _searchStrings ??= new SearchStringRepository(_dbContext);
		public ISearchTermRepository SearchTerms => _searchTerms ??= new SearchTermRepository(_dbContext);
		public ISearchStringTermRepository SearchStringTerms => _searchStringTerms ??= new SearchStringTermRepository(_dbContext);
		public ISearchSourceRepository SearchSources => _searchSources ??= new SearchSourceRepository(_dbContext);

		public IStudySelectionCriteriaRepository SelectionCriterias =>
			_selectionCriterias ??= new StudySelectionCriteriaRepository(_dbContext);

		public IInclusionCriterionRepository InclusionCriteria =>
			_inclusionCriteria ??= new InclusionCriterionRepository(_dbContext);

		public IExclusionCriterionRepository ExclusionCriteria =>
			_exclusionCriteria ??= new ExclusionCriterionRepository(_dbContext);

		public IStudySelectionProcedureRepository SelectionProcedures =>
			_selectionProcedures ??= new StudySelectionProcedureRepository(_dbContext);

		public IQualityAssessmentStrategyRepository QualityStrategies =>
			_qualityStrategies ??= new QualityAssessmentStrategyRepository(_dbContext);

		public IQualityChecklistRepository QualityChecklists =>
			_qualityChecklists ??= new QualityChecklistRepository(_dbContext);

		public IQualityCriterionRepository QualityCriteria =>
			_qualityCriteria ??= new QualityCriterionRepository(_dbContext);

		public IDataExtractionStrategyRepository ExtractionStrategies =>
			_extractionStrategies ??= new DataExtractionStrategyRepository(_dbContext);

		public IDataExtractionFormRepository ExtractionForms =>
			_extractionForms ??= new DataExtractionFormRepository(_dbContext);

		public IDataItemDefinitionRepository DataItems =>
			_dataItems ??= new DataItemDefinitionRepository(_dbContext);

		public IDataSynthesisStrategyRepository SynthesisStrategies =>
			_synthesisStrategies ??= new DataSynthesisStrategyRepository(_dbContext);

		public IDisseminationStrategyRepository DisseminationStrategies =>
			_disseminationStrategies ??= new DisseminationStrategyRepository(_dbContext);

		public IProjectTimetableRepository Timetables =>
			_timetables ??= new ProjectTimetableRepository(_dbContext);

        public ISystematicReviewProjectRepository SystematicReviewProjects
            => _systematicReviewProjects ??= new SystematicReviewProjectRepository(_dbContext);

        public IReviewProcessRepository ReviewProcesses
            => _reviewProcesses ??= new ReviewProcessRepository(_dbContext);

        public IIdentificationProcessRepository IdentificationProcesses
            => _identificationProcesses ??= new IdentificationProcessRepository(_dbContext);

        public ISearchExecutionRepository SearchExecutions
            => _searchExecutions ??= new SearchExecutionRepository(_dbContext);

        public IPaperRepository Papers
            => _papers ??= new PaperRepository(_dbContext);

		public IImportBatchRepository ImportBatches 
			=> _importBatches ??= new ImportBatchRepository(_dbContext);

		public IPrismaReportRepository PrismaReports 
			=> _prismaReports ??= new PrismaReportRepository(_dbContext);

		public IDeduplicationResultRepository DeduplicationResults
			=> _deduplicationResults ??= new DeduplicationResultRepository(_dbContext);

		public IScreeningResolutionRepository ScreeningResolutions
			=> _screeningResolutions ??= new ScreeningResolutionRepository(_dbContext);

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
