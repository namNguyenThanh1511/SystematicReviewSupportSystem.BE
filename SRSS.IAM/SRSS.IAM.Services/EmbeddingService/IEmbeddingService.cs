namespace SRSS.IAM.Services.EmbeddingService
{
    public interface IEmbeddingService
    {
        string ModelName { get; }
        Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken);
    }
}
