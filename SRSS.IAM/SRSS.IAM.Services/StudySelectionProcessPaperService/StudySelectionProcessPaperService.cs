using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.Entities.Enums;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.StudySelection;

namespace SRSS.IAM.Services.StudySelectionProcessPaperService
{
    public class StudySelectionProcessPaperService : IStudySelectionProcessPaperService
    {
        private readonly IUnitOfWork _unitOfWork;

        public StudySelectionProcessPaperService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SaveFinalIncludedPapersAsync(Guid processId, CancellationToken cancellationToken)
        {
            // 1 & 2. Retrieves ScreeningResolution records and extracts PaperIds
            // Filter: Phase == FullText, FinalDecision == Include
            var finalIncludedPaperIds = await _unitOfWork.ScreeningResolutions.GetResolvedPaperIdsByPhaseAsync(
                processId,
                ScreeningPhase.FullText,
                ScreeningDecisionType.Include,
                cancellationToken);

            // 3. Deletes all existing records in StudySelectionProcessPaper for that StudySelectionProcessId
            await _unitOfWork.StudySelectionProcessPapers.DeleteByProcessAsync(processId, cancellationToken);

            // 4 & 5. Inserts new StudySelectionProcessPaper records with new IDs and UTC timestamps
            var newProcessPapers = finalIncludedPaperIds.Select(paperId => new StudySelectionProcessPaper
            {
                Id = Guid.NewGuid(),
                StudySelectionProcessId = processId,
                PaperId = paperId,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            }).ToList();

            if (newProcessPapers.Any())
            {
                await _unitOfWork.StudySelectionProcessPapers.AddRangeAsync(newProcessPapers, cancellationToken);
            }
        }

        public async Task<List<IncludedPaperResponse>> GetIncludedPapersByProcessIdAsync(Guid processId, CancellationToken cancellationToken)
        {
            var includedPapers = await _unitOfWork.StudySelectionProcessPapers.GetWithPaperByProcessAsync(processId, cancellationToken);

            return includedPapers.Select(ip => new IncludedPaperResponse
            {
                Id = ip.Id,
                PaperId = ip.PaperId,
                Title = ip.Paper.Title,
                DOI = ip.Paper.DOI,
                Authors = ip.Paper.Authors,
                PublicationYear = ip.Paper.PublicationYear,
                CreatedAt = ip.CreatedAt
            }).ToList();
        }
    }
}
