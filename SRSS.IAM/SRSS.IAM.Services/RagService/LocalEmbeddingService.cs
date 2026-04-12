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
        // ============================================
        // MODEL IDENTITY CONSTANTS
        // ============================================
        private const string _modelName = "all-MiniLM-L6-v2";
        private const int _dimensions = 384;
        private const string _provider = "SmartComponents.LocalEmbeddings";

        private readonly LocalEmbedder _embedder;
        private readonly ILogger<LocalEmbeddingService> _logger;
        private readonly System.Collections.Concurrent.ConcurrentDictionary<string, float[]> _cache;

        public LocalEmbeddingService(LocalEmbedder embedder, ILogger<LocalEmbeddingService> logger)
        {
            _embedder = embedder ?? throw new ArgumentNullException(nameof(embedder));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = new System.Collections.Concurrent.ConcurrentDictionary<string, float[]>();
        }

        // ============================================
        // MODEL IDENTITY (ILocalEmbeddingService)
        // ============================================

        /// <inheritdoc />
        public string ModelName => _modelName;

        /// <inheritdoc />
        public int Dimensions => _dimensions;

        /// <inheritdoc />
        public string Provider => _provider;

        // ============================================
        // EMBEDDING METHODS (ILocalEmbeddingService)
        // ============================================

        /// <inheritdoc />
        public Vector GetEmbedding(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Text must not be null or whitespace.", nameof(text));
            }

            var cacheKey = NormalizeKey(text);
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                return new Vector(cached);
            }

            var embedding = _embedder.Embed<EmbeddingF32>(text);
            var values = NormalizeL2(embedding.Values.ToArray());

            _cache.TryAdd(cacheKey, values);
            return new Vector(values);
        }

        /// <inheritdoc />
        public List<Vector> GetEmbeddingsBatch(List<string> texts)
        {
            if (texts == null || texts.Count == 0)
            {
                return new List<Vector>();
            }

            var results = new Vector[texts.Count];

            // Use Parallel.ForEach for CPU-bound embedding generation
            Parallel.ForEach(System.Linq.Enumerable.Range(0, texts.Count), i =>
            {
                var text = texts[i];
                if (string.IsNullOrWhiteSpace(text))
                {
                    results[i] = new Vector(new float[_dimensions]);
                    return;
                }

                var cacheKey = NormalizeKey(text);
                if (_cache.TryGetValue(cacheKey, out var cached))
                {
                    results[i] = new Vector(cached);
                    return;
                }

                // Note: SmartComponents.LocalEmbeddings is thread-safe for CPU inference
                var embedding = _embedder.Embed<EmbeddingF32>(text);
                var values = NormalizeL2(embedding.Values.ToArray());

                _cache.TryAdd(cacheKey, values);
                results[i] = new Vector(values);
            });

            return results.ToList();
        }

        private static string NormalizeKey(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            return string.Join(" ",
                input.ToLowerInvariant()
                     .Split(new[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));
        }

        private static float[] NormalizeL2(float[] vector)
        {
            double sum = 0;
            for (int i = 0; i < vector.Length; i++)
            {
                sum += vector[i] * vector[i];
            }

            float precision = (float)Math.Sqrt(sum);
            if (precision > 0)
            {
                for (int i = 0; i < vector.Length; i++)
                {
                    vector[i] /= precision;
                }
            }
            return vector;
        }

        public void Dispose()
        {
            _embedder.Dispose();
        }
    }
}
