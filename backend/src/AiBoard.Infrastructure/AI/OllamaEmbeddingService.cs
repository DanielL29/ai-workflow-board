using System.Net.Http.Json;
using System.Text.Json.Serialization;
using AiBoard.Application.Abstractions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AiBoard.Infrastructure.Options;

namespace AiBoard.Infrastructure.AI;

public sealed class OllamaEmbeddingService(
    IHttpClientFactory httpClientFactory,
    IOptions<OllamaOptions> options,
    DeterministicEmbeddingService fallbackEmbeddingService,
    ILogger<OllamaEmbeddingService> logger) : IEmbeddingService
{
    private readonly OllamaOptions _options = options.Value;

    public async Task<IReadOnlyCollection<float[]>> EmbedAsync(IReadOnlyCollection<string> inputs, CancellationToken cancellationToken)
    {
        if (!_options.Enabled || inputs.Count == 0)
        {
            return await fallbackEmbeddingService.EmbedAsync(inputs, cancellationToken);
        }

        try
        {
            var client = httpClientFactory.CreateClient("ollama");
            var response = await client.PostAsJsonAsync(
                "/api/embed",
                new EmbedRequest(_options.EmbeddingModel, inputs.ToArray(), _options.EmbeddingDimensions),
                cancellationToken);

            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<EmbedResponse>(cancellationToken: cancellationToken);
            return payload?.Embeddings ?? await fallbackEmbeddingService.EmbedAsync(inputs, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falling back from Ollama embeddings to deterministic embeddings.");
            return await fallbackEmbeddingService.EmbedAsync(inputs, cancellationToken);
        }
    }

    private sealed record EmbedRequest(string Model, string[] Input, int Dimensions);

    private sealed record EmbedResponse([property: JsonPropertyName("embeddings")] float[][] Embeddings);
}
