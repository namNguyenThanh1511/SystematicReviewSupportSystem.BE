using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// Persists the raw AI-generated JSON response for study selection criteria generation.
    /// Used for traceability and re-displaying original AI suggestions.
    /// </summary>
    public class StudySelectionCriteriaAIResponse : BaseEntity<Guid>
    {
        public Guid StudySelectionProcessId { get; set; }
        
        /// <summary>
        /// Full raw AI response JSON
        /// </summary>
        public string RawJson { get; set; } = string.Empty;

        // Navigation Properties
        public StudySelectionProcess StudySelectionProcess { get; set; } = null!;
    }
}
