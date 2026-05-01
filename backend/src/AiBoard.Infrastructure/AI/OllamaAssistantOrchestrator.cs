using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AiBoard.Application.Abstractions.AI;
using AiBoard.Application.Boards.Dtos;
using AiBoard.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AiBoard.Infrastructure.AI;

public sealed class OllamaAssistantOrchestrator(
    IHttpClientFactory httpClientFactory,
    IBoardMemoryService boardMemoryService,
    IOptions<OllamaOptions> options,
    ILogger<OllamaAssistantOrchestrator> logger) : IAssistantOrchestrator
{
    private readonly OllamaOptions _options = options.Value;

    public async Task<string> ReplyAsync(Guid? boardId, string message, CancellationToken cancellationToken)
    {
        var context = boardId.HasValue
            ? await boardMemoryService.SearchAsync(boardId.Value, message, 4, cancellationToken)
            : [];

        if (!_options.Enabled)
        {
            return SerializeFallback(boardId, message, context, "ollama-disabled");
        }

        try
        {
            var client = httpClientFactory.CreateClient("ollama");
            var response = await client.PostAsJsonAsync(
                "/api/chat",
                new ChatRequest(
                    _options.ChatModel,
                    [
                        new ChatMessage("system", BuildSystemPrompt(context)),
                        new ChatMessage("user", message)
                    ],
                    false),
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken: cancellationToken);
            var answer = payload?.Message?.Content?.Trim();

            if (string.IsNullOrWhiteSpace(answer))
            {
                return SerializeFallback(boardId, message, context, "ollama-empty-response");
            }

            return JsonSerializer.Serialize(new
            {
                boardId,
                provider = "ollama",
                model = _options.ChatModel,
                answer,
                context
            });
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falling back from Ollama chat to retrieval-only response.");
            return SerializeFallback(boardId, message, context, "ollama-error");
        }
    }

    private static string BuildSystemPrompt(IReadOnlyCollection<BoardMemorySearchResultDto> context)
    {
        var lines = new List<string>
        {
            "You are an assistant for an AI workflow board.",
            "Be concise, practical, and honest about uncertainty."
        };

        if (context.Count == 0)
        {
            lines.Add("No board context was retrieved for this request.");
            return string.Join(Environment.NewLine, lines);
        }

        lines.Add("Use the following retrieved board context when relevant:");
        foreach (var item in context)
        {
            lines.Add($"- {item.Title}: {item.Content}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string SerializeFallback(
        Guid? boardId,
        string message,
        IReadOnlyCollection<BoardMemorySearchResultDto> context,
        string reason)
    {
        return JsonSerializer.Serialize(new
        {
            boardId,
            provider = "fallback",
            reason,
            answer = $"Assistant response seeded with retrieved board context for: {message}",
            context,
            nextSteps = new[]
            {
                "Enable Ollama in configuration to get live model responses",
                "Keep indexing board nodes so retrieval quality improves",
                "Swap to OpenAI later if you need stronger hosted models"
            }
        });
    }

    private sealed record ChatRequest(string Model, ChatMessage[] Messages, bool Stream);
    private sealed record ChatMessage(string Role, string Content);
    private sealed record ChatResponse([property: JsonPropertyName("message")] ChatMessage? Message);
}
