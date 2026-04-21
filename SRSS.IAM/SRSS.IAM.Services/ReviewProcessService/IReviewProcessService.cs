using SRSS.IAM.Services.DTOs.ReviewProcess;
using SRSS.IAM.Repositories.Entities;

namespace SRSS.IAM.Services.ReviewProcessService
{
    public interface IReviewProcessService
    {
        Task<ReviewProcessResponse> CreateReviewProcessAsync(
            Guid projectId,
            CreateReviewProcessRequest request,
            CancellationToken cancellationToken = default);

        Task<ReviewProcessResponse> GetReviewProcessByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<List<ReviewProcessResponse>> GetReviewProcessesByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

        Task<ReviewProcessResponse> UpdateReviewProcessAsync(
            UpdateReviewProcessRequest request,
            CancellationToken cancellationToken = default);

        Task<ReviewProcessResponse> StartReviewProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<ReviewProcessResponse> CompleteReviewProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<ReviewProcessResponse> CancelReviewProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<bool> DeleteReviewProcessAsync(
            Guid id,
            CancellationToken cancellationToken = default);

        Task<ReviewProcessResponse> ReopenPhaseAsync(
            Guid reviewProcessId,
            ProcessPhase phase,
            CancellationToken cancellationToken = default);

        Task<List<ReviewProcessSnapshotResponse>> GetReviewProcessSnapshotsByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

        Task<AddPapersToReviewProcessResponse> AddSelectedPapersAsync(
            Guid reviewProcessId,
            AddSelectedPapersRequest request,
            CancellationToken cancellationToken = default);

        Task<AddPapersFromFilterResponse> AddPapersFromFilterSettingAsync(
            Guid reviewProcessId,
            AddFromFilterSettingRequest request,
            CancellationToken cancellationToken = default);
    }
}
