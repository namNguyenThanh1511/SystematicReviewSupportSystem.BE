using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
    public class SystematicReviewProject : BaseEntity<Guid>
    {
        public string Title { get; set; } = string.Empty;
        public string? Domain { get; set; }
        public string? Description { get; set; }
        public string? ResearchTopic { get; set; }
        public string? ResearchObjective { get; set; }
        public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }

        public Guid OwnerId { get; set; }

        // Navigation Properties
        public ICollection<SearchSource> SearchSources { get; set; } = new List<SearchSource>();
        public ICollection<QualityAssessmentStrategy> QualityStrategies { get; set; } = new List<QualityAssessmentStrategy>();

        public ICollection<ResearchQuestion> ResearchQuestions { get; set; } = new List<ResearchQuestion>();
        public ICollection<ReviewNeed> ReviewNeeds { get; set; } = new List<ReviewNeed>();
        public ICollection<ReviewObjective> ReviewObjectives { get; set; } = new List<ReviewObjective>();
        public ICollection<CommissioningDocument> CommissioningDocuments { get; set; } = new List<CommissioningDocument>();
        public ICollection<ReviewProcess> ReviewProcesses { get; set; } = new List<ReviewProcess>();
        public ICollection<ReviewChecklist> ReviewChecklists { get; set; } = new List<ReviewChecklist>();
        public ICollection<FilterSetting> FilterSettings { get; set; } = new List<FilterSetting>();
        public ICollection<DeduplicationResult> DeduplicationResults { get; set; } = new List<DeduplicationResult>();
        public ICollection<ProjectPicoc> ProjectPicocs { get; set; } = new List<ProjectPicoc>();

        public ICollection<Paper> Papers { get; set; } = new List<Paper>();
        public ICollection<StudySelectionChecklistTemplate> StudySelectionChecklistTemplates { get; set; } = new List<StudySelectionChecklistTemplate>();

        // Domain Methods
        public void Activate()
        {
            if (Status != ProjectStatus.Draft)
            {
                throw new InvalidOperationException($"Cannot activate project from {Status} status.");
            }

            Status = ProjectStatus.Active;
            StartDate = DateTimeOffset.UtcNow;
            ModifiedAt = DateTimeOffset.UtcNow;
        }

        public void Complete()
        {
            if (Status != ProjectStatus.Active)
            {
                throw new InvalidOperationException($"Cannot complete project from {Status} status.");
            }

            if (ReviewProcesses.Any(rp => rp.Status == ProcessStatus.InProgress))
            {
                throw new InvalidOperationException("Cannot complete project while processes are in progress.");
            }

            if (!ReviewProcesses.All(rp => rp.Status == ProcessStatus.Completed))
            {
                throw new InvalidOperationException("Cannot complete project until all processes are completed.");
            }

            Status = ProjectStatus.Completed;
            EndDate = DateTimeOffset.UtcNow;
            ModifiedAt = DateTimeOffset.UtcNow;
        }

        public void Archive()
        {
            if (Status != ProjectStatus.Active && Status != ProjectStatus.Completed)
            {
                throw new InvalidOperationException($"Cannot archive project from {Status} status.");
            }

            Status = ProjectStatus.Archived;
            ModifiedAt = DateTimeOffset.UtcNow;
        }

        public ReviewProcess AddReviewProcess(string name, string? notes = null)
        {
            if (Status == ProjectStatus.Completed || Status == ProjectStatus.Archived)
            {
                throw new InvalidOperationException($"Cannot add processes to project in {Status} status.");
            }

            var reviewProcess = new ReviewProcess
            {
                Id = Guid.NewGuid(),
                Name = name,
                ProjectId = Id,
                Status = ProcessStatus.NotStarted,
                Notes = notes,
                CreatedAt = DateTimeOffset.UtcNow,
                ModifiedAt = DateTimeOffset.UtcNow
            };

            ReviewProcesses.Add(reviewProcess);
            ModifiedAt = DateTimeOffset.UtcNow;

            return reviewProcess;
        }

        public bool CanAddProcess()
        {
            if (Status == ProjectStatus.Completed || Status == ProjectStatus.Archived)
            {
                return false;
            }

            return !ReviewProcesses.Any(rp => rp.Status == ProcessStatus.InProgress);
        }

        public ICollection<ProjectMember> ProjectMembers { get; set; } = new List<ProjectMember>();
        public ICollection<ProjectMemberInvitation> ProjectMemberInvitations { get; set; } = new List<ProjectMemberInvitation>();
    }

    public enum ProjectStatus
    {
        Draft = 0,
        Active = 1,
        Completed = 2,
        Archived = 3
    }
}
