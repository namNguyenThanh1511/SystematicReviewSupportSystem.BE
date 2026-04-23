namespace SRSS.IAM.Services.OpenRouter;

public interface IOpenRouterService
{
    /// <summary>
    /// Gets a full chat completion response from OpenRouter.
    /// </summary>
    Task<string> GetChatResponseAsync(string prompt, string? model = null, double temperature = 0.7, CancellationToken ct = default);

    /// <summary>
    /// Streams chat completion responses from OpenRouter.
    /// </summary>
    IAsyncEnumerable<string> GetStreamingResponseAsync(string prompt, string? model = null, double temperature = 0.7, CancellationToken ct = default);

    /// <summary>
    /// Gets a structured response from OpenRouter and deserializes it to T.
    /// </summary>
    Task<T> GenerateStructuredContentAsync<T>(string prompt, string? model = null, double temperature = 0.7, CancellationToken ct = default);
}
