using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace SRSS.IAM.Services.EmbeddingService
{
    public class GeminiEmbeddingService : IEmbeddingService
    {
        public string ModelName => Model;
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string Model = "models/gemini-embedding-001";
        private const string ApiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-embedding-001:embedContent";

        public GeminiEmbeddingService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _apiKey = configuration["Gemini:ApiKey"] ?? throw new InvalidOperationException("Gemini:ApiKey not found in configuration.");
        }

        public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Array.Empty<float>();
            }

            // Normalize input text (consistent with OpenAIEmbeddingService)
            var normalizedText = text.Trim().Replace("\n", " ").Replace("\r", " ");

            try
            {
                var requestBody = new GeminiEmbeddingRequest
                {
                    Model = Model,
                    Content = new Content
                    {
                        Parts = new[] { new Part { Text = normalizedText } }
                    }
                };

                using var httpRequest = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
                httpRequest.Headers.Add("x-goog-api-key", _apiKey);
                httpRequest.Content = JsonContent.Create(requestBody);

                var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
                
                if (!response.IsSuccessStatusCode)
                {
                    return Array.Empty<float>();
                }

                var result = await response.Content.ReadFromJsonAsync<GeminiEmbeddingResponse>(cancellationToken: cancellationToken);
                return result?.Embedding?.Values ?? Array.Empty<float>();
            }
            catch (Exception)
            {
                // Consistent with OpenAIEmbeddingService: return empty array on failure
                return Array.Empty<float>();
            }
        }

        #region API Models

        private class GeminiEmbeddingRequest
        {
            public string Model { get; set; } = null!;
            public Content Content { get; set; } = null!;
        }

        private class Content
        {
            public Part[] Parts { get; set; } = null!;
        }

        private class Part
        {
            public string Text { get; set; } = null!;
        }

        private class GeminiEmbeddingResponse
        {
            public Embedding? Embedding { get; set; }
        }

        private class Embedding
        {
            public float[] Values { get; set; } = null!;
        }

        #endregion
    }
}
