using AiBoard.Application.Abstractions.AI;

namespace AiBoard.Infrastructure.AI;

public sealed class StubAiGenerationService : IAiGenerationService
{
    public Task<string> GenerateAsync(string provider, string prompt, CancellationToken cancellationToken)
    {
        var normalizedProvider = string.IsNullOrWhiteSpace(provider) ? "mock-provider" : provider.Trim();
        var preview = prompt.Length > 180 ? prompt[..180] : prompt;
        var payload =
            $$"""
            {
              "provider": "{{normalizedProvider}}",
              "type": "mock-result",
              "content": "Generated output for prompt: {{preview.Replace("\"", "\\\"")}}",
              "createdAtUtc": "{{DateTime.UtcNow:O}}"
            }
            """;

        return Task.FromResult(payload);
    }
}
