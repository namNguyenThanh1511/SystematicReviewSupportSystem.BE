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
		public string ProtocolVersion { get; set; } = "1.0";
		public string Status { get; set; } = "Draft"; // Draft, UnderReview, Approved, Rejected
		public DateTimeOffset? ApprovedAt { get; set; }

		// Navigation properties
		public SystematicReviewProject Project { get; set; } = null!;
		public ICollection<ProtocolVersion> Versions { get; set; } = new List<ProtocolVersion>();
		public ICollection<SearchStrategy> SearchStrategies { get; set; } = new List<SearchStrategy>();
		public ICollection<SearchSource> SearchSources { get; set; } = new List<SearchSource>();
		public ICollection<StudySelectionCriteria> SelectionCriterias { get; set; } = new List<StudySelectionCriteria>();
		public ICollection<StudySelectionProcedure> SelectionProcedures { get; set; } = new List<StudySelectionProcedure>();
		public ICollection<ProtocolEvaluation> Evaluations { get; set; } = new List<ProtocolEvaluation>();
		public ICollection<QualityAssessmentStrategy> QualityStrategies { get; set; } = new List<QualityAssessmentStrategy>();
		public ICollection<DataExtractionStrategy> ExtractionStrategies { get; set; } = new List<DataExtractionStrategy>();
		public ICollection<DataSynthesisStrategy> SynthesisStrategies { get; set; } = new List<DataSynthesisStrategy>();
		public ICollection<DisseminationStrategy> DisseminationStrategies { get; set; } = new List<DisseminationStrategy>();
		public ICollection<ProjectTimetable> Timetables { get; set; } = new List<ProjectTimetable>();
	}
}