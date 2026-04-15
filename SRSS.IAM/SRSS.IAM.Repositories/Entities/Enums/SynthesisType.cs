using System.Text.Json.Serialization;

namespace SRSS.IAM.Repositories.Entities.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SynthesisType
    {
        DescriptiveStatistics,
        NarrativeThematic,
        CrossTabulation,
        QuantitativeMetaAnalysis
    }
}
