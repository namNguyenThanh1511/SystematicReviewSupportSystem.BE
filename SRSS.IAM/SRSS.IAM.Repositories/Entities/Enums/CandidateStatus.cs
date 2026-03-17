using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities.Enums
{
    public enum CandidateStatus
    {
        Detected = 0,
        Rejected = 1,
        Imported = 2,
        Duplicate = 3
    }

    public enum PaperSourceType
    {
        DatabaseSearch = 0,
        Snowballing = 1,
        Manual = 2
    }
}
