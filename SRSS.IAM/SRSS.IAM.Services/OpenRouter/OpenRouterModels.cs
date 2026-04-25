using System.Text.Json.Serialization;

namespace SRSS.IAM.Services.OpenRouter;

public record OpenRouterMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content
);

public record OpenRouterChatRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] IEnumerable<OpenRouterMessage> Messages,
    [property: JsonPropertyName("temperature")] double Temperature = 0.7,
    [property: JsonPropertyName("stream")] bool Stream = false,
    [property: JsonPropertyName("response_format")] OpenRouterResponseFormat? ResponseFormat = null
);

public record OpenRouterResponseFormat(
    [property: JsonPropertyName("type")] string Type
);

public record OpenRouterChatResponse(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("choices")] IEnumerable<OpenRouterChoice> Choices,
    [property: JsonPropertyName("created")] long Created,
    [property: JsonPropertyName("model")] string Model
);

public record OpenRouterChoice(
    [property: JsonPropertyName("message")] OpenRouterMessage? Message,
    [property: JsonPropertyName("delta")] OpenRouterDelta? Delta,
    [property: JsonPropertyName("finish_reason")] string? FinishReason
);

public record OpenRouterDelta(
    [property: JsonPropertyName("role")] string? Role,
    [property: JsonPropertyName("content")] string? Content
);

public record OpenRouterErrorResponse(
    [property: JsonPropertyName("error")] OpenRouterError Error
);

public record OpenRouterError(
    [property: JsonPropertyName("message")] string Message,
    [property: JsonPropertyName("code")] int? Code
);
