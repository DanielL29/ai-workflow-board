using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using AiBoard.Application.Abstractions.AI;
using AiBoard.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace AiBoard.Infrastructure.AI;

public sealed class LocalSdWebUiGenerationService(
    IHttpClientFactory httpClientFactory,
    IOptions<LocalImageOptions> options) : IAiGenerationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<string> GenerateAsync(string provider, string prompt, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var client = httpClientFactory.CreateClient("local-image");

        var (providerName, checkpoint) = ParseProvider(provider);
        var payload = new JsonObject
        {
            ["prompt"] = prompt,
            ["steps"] = settings.Steps,
            ["width"] = settings.Width,
            ["height"] = settings.Height,
            ["cfg_scale"] = settings.CfgScale,
            ["sampler_name"] = settings.SamplerName
        };

        if (!string.IsNullOrWhiteSpace(settings.NegativePrompt))
        {
            payload["negative_prompt"] = settings.NegativePrompt;
        }

        if (!string.IsNullOrWhiteSpace(checkpoint))
        {
            payload["override_settings"] = new JsonObject
            {
                ["sd_model_checkpoint"] = checkpoint
            };
        }

        HttpResponseMessage response;
        try
        {
            response = await client.PostAsJsonAsync(settings.Txt2ImgPath, payload, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new InvalidOperationException(
                $"Could not reach local image provider at {settings.BaseUrl}{settings.Txt2ImgPath}. {ex.Message}",
                ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new InvalidOperationException(
                $"Local image provider timed out at {settings.BaseUrl}{settings.Txt2ImgPath}.",
                ex);
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"Local image generation failed at {settings.BaseUrl}{settings.Txt2ImgPath}: {(int)response.StatusCode} {body}");
        }

        var json = JsonNode.Parse(body)?.AsObject() ?? throw new InvalidOperationException("Local image provider returned an empty response.");
        var imageBase64 = json["images"]?.AsArray().FirstOrDefault()?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(imageBase64))
        {
            throw new InvalidOperationException("Local image provider did not return any generated image.");
        }

        var result = new JsonObject
        {
            ["provider"] = providerName,
            ["type"] = "image",
            ["mimeType"] = "image/png",
            ["imageBase64"] = StripDataUrlPrefix(imageBase64),
            ["revisedPrompt"] = prompt,
            ["createdAtUtc"] = DateTime.UtcNow.ToString("O")
        };

        if (!string.IsNullOrWhiteSpace(checkpoint))
        {
            result["model"] = checkpoint;
        }

        return result.ToJsonString(JsonOptions);
    }

    private static (string providerName, string? checkpoint) ParseProvider(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return ("local-sd", null);
        }

        var parts = provider.Split(':', 2, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 2)
        {
            return (parts[0], parts[1]);
        }

        return (provider.Trim(), null);
    }

    private static string StripDataUrlPrefix(string imageBase64)
    {
        var marker = "base64,";
        var index = imageBase64.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        return index >= 0 ? imageBase64[(index + marker.Length)..] : imageBase64;
    }
}
