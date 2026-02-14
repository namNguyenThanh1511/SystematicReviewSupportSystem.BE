using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;

namespace SRSS.IAM.Services.SelectionStatusService
{
    /// <summary>
    /// Service for retrieving paper selection status from ScreeningResolution
    /// Paper entity does NOT store workflow state - it's process-scoped
    /// </summary>
    public interface ISelectionStatusService
    {
        /// <summary>
        /// Get the selection status of a paper for a specific study selection process
        /// Returns Pending if no resolution exists
        /// </summary>
        Task<SelectionStatus> GetPaperSelectionStatusAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get selection statuses for multiple papers in a selection process
        /// </summary>
        Task<Dictionary<Guid, SelectionStatus>> GetPapersSelectionStatusAsync(
            Guid studySelectionProcessId,
            List<Guid> paperIds,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Check if paper is duplicate in an identification process
        /// </summary>
        Task<bool> IsPaperDuplicateAsync(
            Guid identificationProcessId,
            Guid paperId,
            CancellationToken cancellationToken = default);
    }

    public class SelectionStatusService : ISelectionStatusService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SelectionStatusService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SelectionStatus> GetPaperSelectionStatusAsync(
            Guid studySelectionProcessId,
            Guid paperId,
            CancellationToken cancellationToken = default)
        {
            // Check if paper is duplicate first
            var deduplication = await _unitOfWork.DeduplicationResults.FindSingleAsync(
                dr => dr.PaperId == paperId,
                cancellationToken: cancellationToken);

            if (deduplication != null)
            {
                return SelectionStatus.Duplicate;
            }

            // Check screening resolution
            var resolution = await _unitOfWork.ScreeningResolutions.FindSingleAsync(
                sr => sr.StudySelectionProcessId == studySelectionProcessId && sr.PaperId == paperId,
                cancellationToken: cancellationToken);

            if (resolution == null)
            {
                return SelectionStatus.Pending;
            }

            return resolution.FinalDecision == ScreeningDecisionType.Include
                ? SelectionStatus.Included
                : SelectionStatus.Excluded;
        }

        public async Task<Dictionary<Guid, SelectionStatus>> GetPapersSelectionStatusAsync(
            Guid studySelectionProcessId,
            List<Guid> paperIds,
            CancellationToken cancellationToken = default)
        {
            var result = new Dictionary<Guid, SelectionStatus>();

            // Get all resolutions for this process
            var resolutions = await _unitOfWork.ScreeningResolutions.FindAllAsync(
                sr => sr.StudySelectionProcessId == studySelectionProcessId && paperIds.Contains(sr.PaperId),
                cancellationToken: cancellationToken);

            // Get all duplicates
            var duplicates = await _unitOfWork.DeduplicationResults.FindAllAsync(
                dr => paperIds.Contains(dr.PaperId),
                cancellationToken: cancellationToken);

            var resolutionDict = resolutions.ToDictionary(r => r.PaperId);
            var duplicateSet = duplicates.Select(d => d.PaperId).ToHashSet();

            foreach (var paperId in paperIds)
            {
                if (duplicateSet.Contains(paperId))
                {
                    result[paperId] = SelectionStatus.Duplicate;
                }
                else if (resolutionDict.TryGetValue(paperId, out var resolution))
                {
                    result[paperId] = resolution.FinalDecision == ScreeningDecisionType.Include
                        ? SelectionStatus.Included
                        : SelectionStatus.Excluded;
                }
                else
                {
                    result[paperId] = SelectionStatus.Pending;
                }
            }

            return result;
        }

        public async Task<bool> IsPaperDuplicateAsync(
            Guid identificationProcessId,
            Guid paperId,
            CancellationToken cancellationToken = default)
        {
            var deduplication = await _unitOfWork.DeduplicationResults.FindSingleAsync(
                dr => dr.IdentificationProcessId == identificationProcessId && dr.PaperId == paperId,
                cancellationToken: cancellationToken);

            return deduplication != null;
        }
    }
}
