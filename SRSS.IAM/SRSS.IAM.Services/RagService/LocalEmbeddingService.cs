using Microsoft.Extensions.Logging;
using Pgvector;
using SmartComponents.LocalEmbeddings;

namespace SRSS.IAM.Services.RagService
{
    /// <summary>
    /// CPU-local embedding service backed by <c>SmartComponents.LocalEmbeddings</c>
    /// (ONNX BERT model, all-MiniLM-L6-v2, 384 dimensions).
    ///
    /// Registered as a <b>Singleton</b>: the ONNX model is loaded once at startup
    /// and reused across all requests, avoiding repeated model-load latency.
    /// </summary>
    public sealed class LocalEmbeddingService : ILocalEmbeddingService, IDisposable
    {
        private readonly LocalEmbedder _embedder;
        private readonly ILogger<LocalEmbeddingService> _logger;

        public LocalEmbeddingService(LocalEmbedder embedder, ILogger<LocalEmbeddingService> logger)
        {
            _embedder = embedder ?? throw new ArgumentNullException(nameof(embedder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc />
        public Vector GetEmbedding(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Text must not be null or whitespace.", nameof(text));
            }

            var embedding = _embedder.Embed<EmbeddingF32>(text);

            // EmbeddingF32.Values is ReadOnlyMemory<float>; Pgvector.Vector accepts float[]
            return new Vector(embedding.Values.ToArray());
        }

        /// <inheritdoc />
        public List<Vector> GetEmbeddingsBatch(List<string> texts)
        {
            if (texts == null || texts.Count == 0)
            {
                return new List<Vector>();
            }

            var results = new List<Vector>(texts.Count);

            foreach (var text in texts)
            {
                if (string.IsNullOrWhiteSpace(text))
                {
                    _logger.LogWarning("Skipping empty or whitespace text during batch embedding.");
                    // Insert a zero vector as a placeholder to maintain index alignment
                    results.Add(new Vector(new float[384]));
                    continue;
                }

                var embedding = _embedder.Embed<EmbeddingF32>(text);
                results.Add(new Vector(embedding.Values.ToArray()));
            }

            return results;
        }

        public void Dispose()
        {
            _embedder.Dispose();
        }
    }
}
