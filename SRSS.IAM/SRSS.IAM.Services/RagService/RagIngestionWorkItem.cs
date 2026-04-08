using System;

namespace SRSS.IAM.Services.RagService
{
    /// <summary>
    /// Represents a single paper to be processed by the background RAG ingestion pipeline.
    /// </summary>
    public record RagIngestionWorkItem(Guid PaperId, string PdfUrl);
}
