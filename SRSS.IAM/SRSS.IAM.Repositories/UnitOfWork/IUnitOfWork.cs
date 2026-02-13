using Microsoft.EntityFrameworkCore;
using Shared.Repositories;
using SRSS.IAM.Repositories.DataExtractionRepo;
using SRSS.IAM.Repositories.ProtocolRepo;
using SRSS.IAM.Repositories.QualityRepo;
using SRSS.IAM.Repositories.ResearchQuestionRepo;
using SRSS.IAM.Repositories.SearchStrategyRepo;
using SRSS.IAM.Repositories.StudySelectionCriteriaRepo;
using SRSS.IAM.Repositories.SynthesisRepo;
using SRSS.IAM.Repositories.UserRepo;
using SRSS.IAM.Repositories.IdentificationProcessRepo;
using SRSS.IAM.Repositories.SearchExecutionRepo;
using SRSS.IAM.Repositories.PaperRepo;
using SRSS.IAM.Repositories.ImportBatchRepo;
using SRSS.IAM.Repositories.SystematicReviewProjectRepo;
using SRSS.IAM.Repositories.ReviewProcessRepo;

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

		// Protocol
		IReviewProtocolRepository Protocols { get; }
		IProtocolVersionRepository ProtocolVersions { get; }
		IProtocolEvaluationRepository ProtocolEvaluations { get; }

		// Research Question
		IResearchQuestionRepository ResearchQuestions { get; }
		IPicocElementRepository PicocElements { get; }
		IPopulationRepository Populations { get; }
		IInterventionRepository Interventions { get; }
		IComparisonRepository Comparisons { get; }
		IOutcomeRepository Outcomes { get; }
		IContextRepository Contexts { get; }

		// Search Strategy
		ISearchStrategyRepository SearchStrategies { get; }
		ISearchStringRepository SearchStrings { get; }
		ISearchTermRepository SearchTerms { get; }
		ISearchStringTermRepository SearchStringTerms { get; }
		ISearchSourceRepository SearchSources { get; }

		// Selection Criteria
		IStudySelectionCriteriaRepository SelectionCriterias { get; }
		IInclusionCriterionRepository InclusionCriteria { get; }
		IExclusionCriterionRepository ExclusionCriteria { get; }
		IStudySelectionProcedureRepository SelectionProcedures { get; }

		// Quality Assessment
		IQualityAssessmentStrategyRepository QualityStrategies { get; }
		IQualityChecklistRepository QualityChecklists { get; }
		IQualityCriterionRepository QualityCriteria { get; }

		// Data Extraction
		IDataExtractionStrategyRepository ExtractionStrategies { get; }
		IDataExtractionFormRepository ExtractionForms { get; }
		IDataItemDefinitionRepository DataItems { get; }

		// Synthesis
		IDataSynthesisStrategyRepository SynthesisStrategies { get; }
		IDisseminationStrategyRepository DisseminationStrategies { get; }
		IProjectTimetableRepository Timetables { get; }
	}
        ISystematicReviewProjectRepository SystematicReviewProjects { get; }
        IReviewProcessRepository ReviewProcesses { get; }
        IIdentificationProcessRepository IdentificationProcesses { get; }
        ISearchExecutionRepository SearchExecutions { get; }
        IPaperRepository Papers { get; }

        IImportBatchRepository ImportBatches { get; }

        }
}