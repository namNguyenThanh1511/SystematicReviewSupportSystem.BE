using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Mvc;
using Shared.Builder;
using Shared.Models;
using SRSS.IAM.Services.OpenRouter;

namespace SRSS.IAM.API.Controllers;

[ApiController]
[Route("api/test/open-router")]
public class OpenRouterTestController : BaseController
{
    private readonly IOpenRouterService _openRouterService;

    public OpenRouterTestController(IOpenRouterService openRouterService)
    {
        _openRouterService = openRouterService;
    }

    /// <summary>
    /// Tests the non-streaming chat response.
    /// </summary>
    [HttpPost("chat")]
    public async Task<ActionResult<ApiResponse<string>>> TestChat(
        [FromBody] OpenRouterTestRequest request,
        CancellationToken ct)
    {
        var result = await _openRouterService.GetChatResponseAsync(
            request.Prompt, 
            request.Model, 
            request.Temperature, 
            ct);

        return Ok(result, "Chat response received successfully.");
    }

    /// <summary>
    /// Tests the streaming chat response.
    /// </summary>
    /// <remarks>
    /// This endpoint streams the response directly as text/plain chunks.
    /// </remarks>
    [HttpPost("chat-stream")]
    [Produces("text/plain")]
    public async IAsyncEnumerable<string> TestChatStream(
        [FromBody] OpenRouterTestRequest request,
        [FromQuery] bool useSse = false,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        var stream = _openRouterService.GetStreamingResponseAsync(
            request.Prompt, 
            request.Model, 
            request.Temperature, 
            ct);

        await foreach (var chunk in stream)
        {
            if (useSse)
            {
                yield return $"data: {chunk}\n\n";
            }
            else
            {
                yield return chunk;
            }
        }
    }

    /// <summary>
    /// Tests the structured chat response.
    /// </summary>
    [HttpPost("chat-structured")]
    public async Task<ActionResult<ApiResponse<object>>> TestStructuredChat(
        [FromBody] OpenRouterTestRequest request,
        CancellationToken ct)
    {
        // Using object as T for generic testing, 
        // but in real scenarios you would use a specific DTO.
        var result = await _openRouterService.GenerateStructuredContentAsync<object>(
            request.Prompt, 
            request.Model, 
            request.Temperature, 
            ct);

        return Ok(result, "Structured chat response received successfully.");
    }
}

public record OpenRouterTestRequest(
    string Prompt,
    string? Model = null,
    double Temperature = 0.7
);
