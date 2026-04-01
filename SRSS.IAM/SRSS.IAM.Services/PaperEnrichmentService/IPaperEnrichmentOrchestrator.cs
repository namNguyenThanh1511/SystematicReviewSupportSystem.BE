using System;
using System.Threading;
using System.Threading.Tasks;

namespace SRSS.IAM.Services.PaperEnrichmentService
{
    /// <summary>
    /// Orchestrates downstream-driven paper enrichment.
    /// Triggered after identification process completion to enrich only final dataset papers.
    /// </summary>
    public interface IPaperEnrichmentOrchestrator
    {
        /// <summary>
        /// Triggers enrichment for all eligible papers in the final dataset
        /// of the given identification process. Enqueues papers to background processing.
        /// </summary>
        Task TriggerForIdentificationProcessAsync(Guid identificationProcessId, CancellationToken ct = default);
    }
}
