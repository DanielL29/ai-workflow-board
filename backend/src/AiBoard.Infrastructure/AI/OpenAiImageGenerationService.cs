using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AiBoard.Application.Abstractions.AI;
using AiBoard.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace AiBoard.Infrastructure.AI;

public sealed class OpenAiImageGenerationService(
    IHttpClientFactory httpClientFactory,
    IOptions<OpenAiOptions> options) : IAiGenerationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<string> GenerateAsync(string provider, string prompt, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var client = httpClientFactory.CreateClient("openai");

        using var request = new HttpRequestMessage(HttpMethod.Post, "images/generations");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);

        var payload = new
        {
            model = ResolveModel(provider, settings.ImageModel),
            prompt,
            size = settings.Size,
            quality = settings.Quality,
            background = settings.Background,
            output_format = settings.OutputFormat
        };

        request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        using var response = await client.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"OpenAI image generation failed: {(int)response.StatusCode} {body}");
        }

        var json = JsonNode.Parse(body)?.AsObject() ?? throw new InvalidOperationException("OpenAI returned an empty response.");
        var imageData = json["data"]?.AsArray().FirstOrDefault()?.AsObject()
            ?? throw new InvalidOperationException("OpenAI response did not include image data.");

        var imageBase64 = imageData["b64_json"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(imageBase64))
        {
            throw new InvalidOperationException("OpenAI response did not include b64_json image content.");
        }

        var result = new JsonObject
        {
            ["provider"] = "openai",
            ["type"] = "image",
            ["mimeType"] = $"image/{settings.OutputFormat}",
            ["imageBase64"] = imageBase64,
            ["revisedPrompt"] = imageData["revised_prompt"]?.GetValue<string>() ?? prompt,
            ["createdAtUtc"] = DateTime.UtcNow.ToString("O")
        };

        return result.ToJsonString(JsonOptions);
    }

    private static string ResolveModel(string provider, string fallbackModel)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return fallbackModel;
        }

        var trimmed = provider.Trim();
        return trimmed.Equals("openai", StringComparison.OrdinalIgnoreCase) ? fallbackModel : trimmed;
    }
}
