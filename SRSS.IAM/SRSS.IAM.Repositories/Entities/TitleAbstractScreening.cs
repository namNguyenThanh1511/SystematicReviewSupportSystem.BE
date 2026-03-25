using Shared.Entities.BaseEntity;
using SRSS.IAM.Repositories.Entities.Enums;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Represents the Title/Abstract screening phase (Phase 1) of study selection.
    /// Child entity of StudySelectionProcess (1:1 relationship).
    /// </summary>
    public class TitleAbstractScreening : BaseEntity<Guid>
    {
        public Guid StudySelectionProcessId { get; set; }
        public ScreeningPhaseStatus Status { get; set; } = ScreeningPhaseStatus.NotStarted;
        public DateTimeOffset? StartedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
        public int MinReviewersPerPaper { get; set; } = 2;
        public int MaxReviewersPerPaper { get; set; } = 3;

        // Navigation Properties
        public StudySelectionProcess StudySelectionProcess { get; set; } = null!;

        // Domain Methods
        public void Start()
        {
            if (Status != ScreeningPhaseStatus.NotStarted)
            {
                throw new InvalidOperationException($"Cannot start Title/Abstract screening from {Status} status.");
            }

            Status = ScreeningPhaseStatus.InProgress;
            StartedAt = DateTimeOffset.UtcNow;
            ModifiedAt = DateTimeOffset.UtcNow;
        }

        public void Complete()
        {
            if (Status != ScreeningPhaseStatus.InProgress)
            {
                throw new InvalidOperationException($"Cannot complete Title/Abstract screening from {Status} status.");
            }

            Status = ScreeningPhaseStatus.Completed;
            CompletedAt = DateTimeOffset.UtcNow;
            ModifiedAt = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Validates that a paper has the minimum required metadata for TA screening.
        /// Title is always required (non-nullable); checks Abstract, Year, Language.
        /// </summary>
        public static void ValidatePaperMetadata(Paper paper)
        {
            var missing = new List<string>();

            if (string.IsNullOrWhiteSpace(paper.Title))
                missing.Add("Title");
            if (string.IsNullOrWhiteSpace(paper.Abstract))
                missing.Add("Abstract");
            if (string.IsNullOrWhiteSpace(paper.PublicationYear) && paper.PublicationYearInt == null)
                missing.Add("PublicationYear");
            if (string.IsNullOrWhiteSpace(paper.Language))
                missing.Add("Language");

            if (missing.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Paper '{paper.Title}' (ID: {paper.Id}) is missing required metadata for screening: {string.Join(", ", missing)}.");
            }
        }
    }
}
