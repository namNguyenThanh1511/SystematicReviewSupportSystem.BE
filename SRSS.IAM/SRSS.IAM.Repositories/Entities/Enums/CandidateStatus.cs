using Shared.Entities.BaseEntity;

namespace SRSS.IAM.Repositories.Entities.Enums
{
    public enum CandidateStatus
    {
        Detected = 0,
        Matched = 1,
        Rejected = 2,
        Suggested = 3

    }

    public enum PaperSourceType
    {
        DatabaseSearch = 0,
        Snowballing = 1,
        Manual = 2
    }
}
