using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SRSS.IAM.Services.OpenRouter;

public class OpenRouterService : IOpenRouterService
{
    private readonly HttpClient _httpClient;
    private readonly OpenRouterOptions _options;
    private readonly ILogger<OpenRouterService> _logger;

    public OpenRouterService(
        HttpClient httpClient,
        IOptions<OpenRouterOptions> options,
        ILogger<OpenRouterService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        if (_httpClient.BaseAddress == null)
        {
            _httpClient.BaseAddress = new Uri(_options.BaseUrl.EndsWith("/") ? _options.BaseUrl : _options.BaseUrl + "/");
        }

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);

        if (!string.IsNullOrEmpty(_options.SiteUrl))
        {
            _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", _options.SiteUrl);
        }

        if (!string.IsNullOrEmpty(_options.SiteName))
        {
            _httpClient.DefaultRequestHeaders.Add("X-Title", _options.SiteName);
        }
    }

    public async Task<string> GetChatResponseAsync(string prompt, string? model = null, double temperature = 0.7, CancellationToken ct = default)
    {
        var request = new OpenRouterChatRequest(
            Model: model ?? _options.DefaultModel,
            Messages: new[] { new OpenRouterMessage("user", prompt) },
            Temperature: temperature,
            Stream: false
        );

        try
        {
            var response = await _httpClient.PostAsJsonAsync("chat/completions", request, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<OpenRouterErrorResponse>(cancellationToken: ct);
                var message = error?.Error?.Message ?? "Unknown error from OpenRouter";
                _logger.LogError("OpenRouter API error: {Message} (Status: {Status})", message, response.StatusCode);
                throw new InvalidOperationException($"OpenRouter API error: {message}");
            }

            var result = await response.Content.ReadFromJsonAsync<OpenRouterChatResponse>(cancellationToken: ct);
            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Exception occurred while calling OpenRouter API");
            throw;
        }
    }

    public async IAsyncEnumerable<string> GetStreamingResponseAsync(
        string prompt,
        string? model = null,
        double temperature = 0.7,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var requestData = new OpenRouterChatRequest(
            Model: model ?? _options.DefaultModel,
            Messages: new[] { new OpenRouterMessage("user", prompt) },
            Temperature: temperature,
            Stream: true
        );

        using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        request.Content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");

        using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync(ct);
            _logger.LogError("OpenRouter Streaming API error: {Body} (Status: {Status})", errorBody, response.StatusCode);
            throw new InvalidOperationException($"OpenRouter Streaming API error: {response.StatusCode}");
        }

        using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(ct);
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.StartsWith("data: "))
            {
                var data = line["data: ".Length..];

                if (data == "[DONE]") break;

                OpenRouterChatResponse? chunk = null;
                try
                {
                    chunk = JsonSerializer.Deserialize<OpenRouterChatResponse>(data);
                }
                catch (JsonException)
                {
                    _logger.LogWarning("Failed to deserialize OpenRouter stream chunk: {Data}", data);
                    continue;
                }

                var content = chunk?.Choices?.FirstOrDefault()?.Delta?.Content;
                if (!string.IsNullOrEmpty(content))
                {
                    yield return content;
                }
            }
        }
    }

    public async Task<T> GenerateStructuredContentAsync<T>(string prompt, string? model = null, double temperature = 0.7, CancellationToken ct = default)
    {
        var schema = GenerateJsonSchema(typeof(T));
        
        // Log the schema for debugging
        _logger.LogInformation("Generated JSON Schema for {Type}: {Schema}", typeof(T).Name, JsonSerializer.Serialize(schema));

        var request = new OpenRouterChatRequest(
            Model: model ?? _options.DefaultModel,
            Messages: new[] { new OpenRouterMessage("user", prompt) },
            Temperature: temperature,
            Stream: false,
            ResponseFormat: new OpenRouterResponseFormat
            {
                Type = "json_schema",
                JsonSchema = new OpenRouterJsonSchema
                {
                    Name = typeof(T).Name.ToLowerInvariant(),
                    Strict = true,
                    Schema = schema ?? new { type = "object", additionalProperties = false }
                }
            }
        );

        try
        {
            var response = await _httpClient.PostAsJsonAsync("chat/completions", request, ct);

            // BƯỚC 1: Đọc content vào biến string DUY NHẤT
            var rawJsonResponse = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                // Thay vì ReadFromJsonAsync, hãy Deserialize từ chuỗi rawJsonResponse đã có
                var error = JsonSerializer.Deserialize<OpenRouterErrorResponse>(rawJsonResponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                var message = error?.Error?.Message ?? "Unknown error from OpenRouter";
                throw new InvalidOperationException($"OpenRouter API error: {message}");
            }

            // BƯỚC 2: Deserialize từ chuỗi rawJsonResponse (KHÔNG dùng ReadFromJsonAsync ở đây)
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
            options.Converters.Add(new FlexibleStringConverter());
            options.Converters.Add(new FlexibleGuidConverter());

            var result = JsonSerializer.Deserialize<OpenRouterChatResponse>(rawJsonResponse, options);

            var content = result?.Choices?.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrEmpty(content))
            {
                throw new InvalidOperationException("OpenRouter returned an empty response.");
            }

            // BƯỚC 3: Deserialize nội dung AI vào kiểu T
            return JsonSerializer.Deserialize<T>(content, options)!;
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Exception occurred while calling OpenRouter for structured content");
            throw;
        }
    }

    private static object? GenerateJsonSchema(System.Type type, HashSet<Type>? seenTypes = null)
    {
        seenTypes ??= new HashSet<Type>();
        
        var underlyingType = Nullable.GetUnderlyingType(type);
        var actualType = underlyingType ?? type;

        var isNullable = underlyingType != null || !type.IsValueType;
        
        if (actualType == typeof(string) || actualType == typeof(Guid)) 
            return new { type = isNullable ? new[] { "string", "null" } : (object)"string" };
        if (actualType == typeof(int) || actualType == typeof(long) || actualType == typeof(short)) 
            return new { type = isNullable ? new[] { "integer", "null" } : (object)"integer" };
        if (actualType.IsEnum) 
            return new { type = isNullable ? new[] { "integer", "null" } : (object)"integer" };
        if (actualType == typeof(float) || actualType == typeof(double) || actualType == typeof(decimal)) 
            return new { type = isNullable ? new[] { "number", "null" } : (object)"number" };
        if (actualType == typeof(bool)) 
            return new { type = isNullable ? new[] { "boolean", "null" } : (object)"boolean" };

        if (typeof(System.Collections.IEnumerable).IsAssignableFrom(actualType) && actualType != typeof(string))
        {
            Type? elementType = null;
            if (actualType.IsArray)
            {
                elementType = actualType.GetElementType();
            }
            else if (actualType.IsGenericType)
            {
                elementType = actualType.GetGenericArguments()[0];
            }
            else
            {
                elementType = typeof(object);
            }

            // Detect recursion in collections
            if (elementType != null && seenTypes.Contains(elementType))
            {
                return null; // Skip recursive collection
            }

            var itemSchema = GenerateJsonSchema(elementType ?? typeof(object), new HashSet<Type>(seenTypes));
            if (itemSchema == null) return null;

            return new
            {
                type = "array",
                items = itemSchema
            };
        }

        if (seenTypes.Contains(actualType))
        {
            return null; // Recursion detected
        }

        var newSeenTypes = new HashSet<Type>(seenTypes) { actualType };
        var properties = new Dictionary<string, object>();
        var required = new List<string>();

        foreach (var prop in actualType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
        {
            // Skip properties that might cause issues or are ignored by JSON
            if (prop.GetCustomAttribute<JsonIgnoreAttribute>() != null) continue;

            var propSchema = GenerateJsonSchema(prop.PropertyType, newSeenTypes);
            if (propSchema != null)
            {
                var attr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();
                string propName = attr?.Name ?? char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1);

                properties[propName] = propSchema;
                required.Add(propName);
            }
        }

        return new
        {
            type = "object",
            properties = properties,
            required = required,
            additionalProperties = false
        };
    }

    private static string CleanJsonString(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return content;

        content = content.Trim();

        // 1. Extract JSON object or array
        int firstBrace = content.IndexOf('{');
        int lastBrace = content.LastIndexOf('}');
        int firstBracket = content.IndexOf('[');
        int lastBracket = content.LastIndexOf(']');

        int start = -1;
        int end = -1;

        if (firstBrace >= 0 && (firstBracket < 0 || firstBrace < firstBracket))
        {
            start = firstBrace;
            end = lastBrace;
        }
        else if (firstBracket >= 0)
        {
            start = firstBracket;
            end = lastBracket;
        }

        if (start >= 0 && end > start)
        {
            content = content.Substring(start, end - start + 1);
        }

        // 2. Escape literal newlines inside strings
        return EscapeLiteralNewlines(content);
    }

    private static string EscapeLiteralNewlines(string json)
    {
        var sb = new StringBuilder();
        bool isInsideString = false;
        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];
            // Check for unescaped double quotes to toggle isInsideString
            if (c == '"' && (i == 0 || json[i - 1] != '\\'))
            {
                isInsideString = !isInsideString;
                sb.Append(c);
            }
            // If inside a string, escape literal newlines
            else if (isInsideString && (c == '\n' || c == '\r'))
            {
                sb.Append("\\n");
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}

public class FlexibleStringConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }

        if (reader.TokenType == JsonTokenType.Number ||
            reader.TokenType == JsonTokenType.True ||
            reader.TokenType == JsonTokenType.False)
        {
            return Encoding.UTF8.GetString(reader.ValueSpan);
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return Encoding.UTF8.GetString(reader.ValueSpan);
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

public class FlexibleGuidConverter : JsonConverter<Guid?>
{
    public override Guid? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            if (Guid.TryParse(reader.GetString(), out var guid))
                return guid;
            return null;
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            Guid? firstGuid = null;
            reader.Read(); // Move to first element

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (firstGuid == null && reader.TokenType == JsonTokenType.String)
                {
                    if (Guid.TryParse(reader.GetString(), out var guid))
                        firstGuid = guid;
                }
                reader.Read();
            }
            return firstGuid;
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, Guid? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value.ToString());
        else
            writer.WriteNullValue();
    }
}
