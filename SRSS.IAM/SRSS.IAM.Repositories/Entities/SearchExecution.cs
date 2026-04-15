    using Shared.Entities.BaseEntity;

    namespace SRSS.IAM.Repositories.Entities
    {
        public class SearchExecution : BaseEntity<Guid>
        {
            public Guid IdentificationProcessId { get; set; }
            public Guid SearchSourceId { get; set; }
            public string? SearchQuery { get; set; }
            public DateTimeOffset ExecutedAt { get; set; }
            public int ResultCount { get; set; }
            public SearchExecutionType Type { get; set; }
            public string? Notes { get; set; }

            public IdentificationProcess IdentificationProcess { get; set; } = default!;
            public ICollection<ImportBatch> ImportBatches { get; set; } = new List<ImportBatch>();
            public SearchSource SearchSource { get; set; } = default!;
        }

        public enum SearchExecutionType
        {
            DatabaseSearch = 0,
            ManualImport = 1
        }
    }
