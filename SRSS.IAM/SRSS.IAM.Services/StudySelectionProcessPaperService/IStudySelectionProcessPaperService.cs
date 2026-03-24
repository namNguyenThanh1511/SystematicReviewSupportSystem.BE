namespace SRSS.IAM.Services.StudySelectionProcessPaperService
{
    public interface IStudySelectionProcessPaperService
    {
        Task SaveFinalIncludedPapersAsync(Guid processId, CancellationToken cancellationToken);
    }
}
