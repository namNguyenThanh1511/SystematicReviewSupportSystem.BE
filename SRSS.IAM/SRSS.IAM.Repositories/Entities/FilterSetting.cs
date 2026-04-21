using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities
{
    public class FilterSetting : BaseEntity<Guid>
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? SearchText { get; set; }
        public string? Keyword { get; set; }
        public int? YearFrom { get; set; }
        public int? YearTo { get; set; }
        public Guid? SearchSourceId { get; set; }
        public Guid? ImportBatchId { get; set; }
        public string DoiState { get; set; } = "all";
        public string FullTextState { get; set; } = "all";
        public bool OnlyUnused { get; set; }
        public bool RecentlyImported { get; set; }

        public SystematicReviewProject Project { get; set; } = null!;
        public ICollection<ReviewProcess> ReviewProcesses { get; set; } = new List<ReviewProcess>();
    }
}
