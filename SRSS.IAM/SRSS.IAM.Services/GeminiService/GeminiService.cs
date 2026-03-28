using Microsoft.Extensions.Configuration;
using System.Collections;
using System.Text.Json;
using Google.GenAI;
using Google.GenAI.Types;
using GeminiType = Google.GenAI.Types.Type;
using System.Reflection;
using System.Text.Json.Serialization; // Prevent naming collision

namespace SRSS.IAM.Services.GeminiService
{
    public class GeminiService : IGeminiService
    {
        private readonly string _modelId;
        private readonly Client _client;

        public GeminiService(IConfiguration configuration)
        {
            var apiKey = configuration["Gemini:ApiKey"] ?? throw new ArgumentNullException("Gemini API Key is not configured.");
            _client = new Client(apiKey: apiKey);
            _modelId = configuration["Gemini:ModelId"] ?? "gemini-2.5-pro";
        }

        public async Task<string> GenerateContentAsync(string prompt)
        {
            var response = await _client.Models.GenerateContentAsync(
                model: _modelId,
                contents: prompt
            );

            return response.Text ?? string.Empty;
        }

        public async Task<T> GenerateStructuredContentAsync<T>(string prompt)
        {
            var config = new GenerateContentConfig
            {
                ResponseSchema = GenerateSchema<T>(),
                ResponseMimeType = "application/json"
            };

            var response = await _client.Models.GenerateContentAsync(
                model: _modelId,
                contents: prompt,
                config: config
            );

            if (string.IsNullOrEmpty(response.Text))
            {
                throw new Exception("Received empty response from Gemini API.");
            }

            return JsonSerializer.Deserialize<T>(response.Text, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public static Schema GenerateSchema<T>() => GenerateSchema(typeof(T));

        private static Schema GenerateSchema(System.Type type)
        {
            // Handle Nullable types
            var underlyingType = Nullable.GetUnderlyingType(type);
            var actualType = underlyingType ?? type;

            // 1. Handle Primitives
            if (actualType == typeof(string) || actualType == typeof(Guid)) return new Schema { Type = GeminiType.String };
            if (actualType == typeof(int) || actualType == typeof(long) || actualType == typeof(short)) return new Schema { Type = GeminiType.Integer };
            if (actualType.IsEnum) return new Schema { Type = GeminiType.Integer }; // Enum as integer
            if (actualType == typeof(float) || actualType == typeof(double) || actualType == typeof(decimal)) return new Schema { Type = GeminiType.Number };
            if (actualType == typeof(bool)) return new Schema { Type = GeminiType.Boolean };

            // 2. Handle Arrays/Lists
            if (typeof(IEnumerable).IsAssignableFrom(actualType) && actualType != typeof(string))
            {
                var elementType = actualType.IsArray ? actualType.GetElementType() : actualType.GetGenericArguments()[0];
                return new Schema
                {
                    Type = GeminiType.Array,
                    Items = GenerateSchema(elementType)
                };
            }

            // 3. Handle Complex Objects (Classes/Structs)
            var schema = new Schema
            {
                Type = GeminiType.Object,
                Properties = new Dictionary<string, Schema>()
            };

            foreach (var prop in actualType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                // Optional: Respect [JsonPropertyName] if you use it!
                var attr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                string propName = attr?.Name ?? char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1);

                schema.Properties[propName] = GenerateSchema(prop.PropertyType);
            }

            return schema;
        }
    }
}