using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using AiBoard.Application.Abstractions.AI;
using AiBoard.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace AiBoard.Infrastructure.AI;

public sealed class StabilityImageGenerationService(
    IHttpClientFactory httpClientFactory,
    IOptions<StabilityOptions> options) : IAiGenerationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<string> GenerateAsync(string provider, string prompt, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var client = httpClientFactory.CreateClient("stability");

        using var request = new HttpRequestMessage(HttpMethod.Post, $"v1/generation/{settings.Engine}/text-to-image");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);

        var payload = new
        {
            text_prompts = new[] { new { text = prompt } },
            cfg_scale = 7.0,
            height = settings.Height,
            width = settings.Width,
            samples = 1
        };

        request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        using var response = await client.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Stability image generation failed: {(int)response.StatusCode} {body}");
        }

        var json = JsonNode.Parse(body)?.AsObject() ?? throw new InvalidOperationException("Stability returned an empty response.");

        // Try common response shapes: `artifacts[0].base64` or `data[0].b64_json`
        string? imageBase64 = null;
        try
        {
            imageBase64 = json["artifacts"]?.AsArray()?.FirstOrDefault()?.AsObject()?["base64"]?.GetValue<string>();
        }
        catch { }

        if (string.IsNullOrWhiteSpace(imageBase64))
        {
            imageBase64 = json["data"]?.AsArray()?.FirstOrDefault()?.AsObject()?["b64_json"]?.GetValue<string>();
        }

        if (string.IsNullOrWhiteSpace(imageBase64))
        {
            throw new InvalidOperationException("Stability response did not include base64 image content.");
        }

        var result = new JsonObject
        {
            ["provider"] = "stability",
            ["type"] = "image",
            ["mimeType"] = "image/png",
            ["imageBase64"] = imageBase64,
            ["revisedPrompt"] = prompt,
            ["createdAtUtc"] = DateTime.UtcNow.ToString("O")
        };

        return result.ToJsonString(JsonOptions);
    }
}
