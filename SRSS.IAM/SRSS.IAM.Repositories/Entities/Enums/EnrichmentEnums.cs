namespace SRSS.IAM.Repositories.Entities.Enums
{
    /// <summary>
    /// Tracks the enrichment state of a paper via external APIs (e.g. OpenAlex).
    /// Used for idempotency and retry logic.
    /// </summary>
    public enum EnrichmentStatus
    {
        NotStarted = 0,
        Processing = 1,
        Completed = 2,
        Failed = 3
    }
}
