namespace SRSS.IAM.Services.PaperFullTextService
{
    public interface IPaperFullTextPreparationService
    {
        /// <summary>
        /// Prepares a paper's full text for AI analysis by performing chunking and embedding.
        /// </summary>
        Task PrepareForAiAsync(Guid paperPdfId, CancellationToken cancellationToken = default);
    }
}
