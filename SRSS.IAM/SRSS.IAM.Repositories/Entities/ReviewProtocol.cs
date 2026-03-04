using Shared.Entities.BaseEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRSS.IAM.Repositories.Entities
{
	public class ReviewProtocol : BaseEntity<Guid>
	{
		public Guid ProjectId { get; set; }
		public string ProtocolVersion { get; set; } = "1.0.0";
		public ProtocolStatus Status { get; set; } = ProtocolStatus.Draft;
		public DateTimeOffset? ApprovedAt { get; set; }
		public bool IsDeleted { get; set; } = false;
		public DateTimeOffset? DeletedAt { get; set; }

		// Navigation properties
		public SystematicReviewProject Project { get; set; } = null!;
		public ICollection<ProtocolVersion> Versions { get; set; } = new List<ProtocolVersion>();
		public ICollection<SearchSource> SearchSources { get; set; } = new List<SearchSource>();
		public ICollection<StudySelectionCriteria> SelectionCriterias { get; set; } = new List<StudySelectionCriteria>();
		public ICollection<StudySelectionProcedure> SelectionProcedures { get; set; } = new List<StudySelectionProcedure>();
		public ICollection<ProtocolEvaluation> Evaluations { get; set; } = new List<ProtocolEvaluation>();
		public ICollection<QualityAssessmentStrategy> QualityStrategies { get; set; } = new List<QualityAssessmentStrategy>();
		public ICollection<DataExtractionStrategy> ExtractionStrategies { get; set; } = new List<DataExtractionStrategy>();
		public ICollection<DataSynthesisStrategy> SynthesisStrategies { get; set; } = new List<DataSynthesisStrategy>();
		public ICollection<DisseminationStrategy> DisseminationStrategies { get; set; } = new List<DisseminationStrategy>();
		public ICollection<ProjectTimetable> Timetables { get; set; } = new List<ProjectTimetable>();

		// Domain Methods
		public void Approve(Guid userId)
		{
			if (Status == ProtocolStatus.Approved)
			{
				throw new InvalidOperationException("Protocol đã được phê duyệt trước đó.");
			}

			if (IsDeleted)
			{
				throw new InvalidOperationException("Không thể phê duyệt protocol đã bị xóa.");
			}

			Status = ProtocolStatus.Approved;
			ApprovedAt = DateTimeOffset.UtcNow;
			ModifiedAt = DateTimeOffset.UtcNow;

			// Create evaluation record with User ID
			//var evaluation = new ProtocolEvaluation
			//{
			//	Id = Guid.NewGuid(),
			//	ProtocolId = Id,
			//	ReviewerId = userId, // Direct User ID
			//	EvaluationResult = "Approved",
			//	Comment = "Protocol approved",
			//	EvaluatedAt = DateTimeOffset.UtcNow,
			//	CreatedAt = DateTimeOffset.UtcNow,
			//	ModifiedAt = DateTimeOffset.UtcNow
			//};

			//Evaluations.Add(evaluation);
		}

		public void Reject(Guid userId, string? reason = null)
		{
			if (Status == ProtocolStatus.Approved)
			{
				throw new InvalidOperationException("Không thể reject protocol đã được phê duyệt.");
			}

			if (IsDeleted)
			{
				throw new InvalidOperationException("Không thể reject protocol đã bị xóa.");
			}

			Status = ProtocolStatus.Rejected;
			ModifiedAt = DateTimeOffset.UtcNow;

			// Create evaluation record with User ID
			//var evaluation = new ProtocolEvaluation
			//{
			//	Id = Guid.NewGuid(),
			//	ProtocolId = Id,
			//	ReviewerId = userId, // Direct User ID
			//	EvaluationResult = "Rejected",
			//	Comment = reason ?? "Protocol rejected",
			//	EvaluatedAt = DateTimeOffset.UtcNow,
			//	CreatedAt = DateTimeOffset.UtcNow,
			//	ModifiedAt = DateTimeOffset.UtcNow
			//};

			//Evaluations.Add(evaluation);
		}

		public void SubmitForReview()
		{
			if (Status != ProtocolStatus.Draft)
			{
				throw new InvalidOperationException($"Chỉ có thể submit protocol ở trạng thái Draft. Hiện tại: {Status}");
			}

			if (IsDeleted)
			{
				throw new InvalidOperationException("Không thể submit protocol đã bị xóa.");
			}

			Status = ProtocolStatus.UnderReview;
			ModifiedAt = DateTimeOffset.UtcNow;
		}

		public void SoftDelete()
		{
			if (Status == ProtocolStatus.Approved)
			{
				throw new InvalidOperationException("Không thể xóa protocol đã được phê duyệt.");
			}

			if (IsDeleted)
			{
				throw new InvalidOperationException("Protocol đã bị xóa trước đó.");
			}

			IsDeleted = true;
			DeletedAt = DateTimeOffset.UtcNow;
			ModifiedAt = DateTimeOffset.UtcNow;
		}

		public void Restore()
		{
			if (!IsDeleted)
			{
				throw new InvalidOperationException("Protocol chưa bị xóa.");
			}

			IsDeleted = false;
			DeletedAt = null;
			ModifiedAt = DateTimeOffset.UtcNow;
		}

		public bool CanDelete()
		{
			return Status != ProtocolStatus.Approved && !IsDeleted;
		}

		public bool CanEdit()
		{
			return Status == ProtocolStatus.Draft && !IsDeleted;
		}
	}

	public enum ProtocolStatus
	{
		Draft = 0,
		UnderReview = 1,
		Approved = 2,
		Rejected = 3,
		NeedsRevision = 4
	}
}