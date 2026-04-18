using System.Text.Json.Serialization;

namespace SRSS.IAM.Repositories.Entities.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SynthesisProcessStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Reopened
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FindingStatus
    {
        Draft,
        Finalized
    }
}
