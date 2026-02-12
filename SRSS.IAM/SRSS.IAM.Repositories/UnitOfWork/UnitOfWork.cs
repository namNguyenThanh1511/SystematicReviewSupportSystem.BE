using SRSS.IAM.Repositories.DataExtractionRepo;
using SRSS.IAM.Repositories.ProtocolRepo;
using SRSS.IAM.Repositories.QualityRepo;
using SRSS.IAM.Repositories.ResearchQuestionRepo;
using SRSS.IAM.Repositories.SearchStrategyRepo;
using SRSS.IAM.Repositories.StudySelectionCriteriaRepo;
using SRSS.IAM.Repositories.SynthesisRepo;
using SRSS.IAM.Repositories.UserRepo;

namespace SRSS.IAM.Repositories.UnitOfWork
{
    public class UnitOfWork : Shared.Repositories.UnitOfWork<AppDbContext>, IUnitOfWork
    {

		private IUserRepository? _users;
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
		private IStudySelectionProcedureRepository _selectionProcedures;
		private IQualityAssessmentStrategyRepository? _qualityStrategies;
		private IQualityChecklistRepository? _qualityChecklists;
		private IQualityCriterionRepository? _qualityCriteria;
		private IDataExtractionStrategyRepository? _extractionStrategies;
		private IDataExtractionFormRepository? _extractionForms;
		private IDataItemDefinitionRepository? _dataItems;
		private IDataSynthesisStrategyRepository? _synthesisStrategies;
		private IDisseminationStrategyRepository? _disseminationStrategies;
		private IProjectTimetableRepository? _timetables;

		public UnitOfWork(AppDbContext dbContext) : base(dbContext)
        {
            //_dbContext được quản lí bởi lớp cha
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

	}
}