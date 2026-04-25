using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Services.DTOs.PrismaReport;

namespace SRSS.IAM.Services.DTOs.Identification
{
    public class PrismaStatisticsResponse
    {
        public int TotalRecordsImported { get; set; }
        public int DuplicateRecords { get; set; }
        public int UniqueRecords { get; set; }
        public int PendingSelectionCount { get; set; }
        public int ImportBatchCount { get; set; }
        public List<PrismaBreakdownResponse> IdentifiedBreakdown { get; set; } = new();
    }
}
