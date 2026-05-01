using System.Text.Json.Nodes;
using AiBoard.Application.Abstractions.AI;

namespace AiBoard.Application.AI;

public static class GeneratedOutputNormalizer
{
    public static async Task<string> NormalizeAsync(
        string rawOutput,
        IGeneratedImageStore imageStore,
        CancellationToken cancellationToken)
    {
        JsonObject? payload;

        try
        {
            payload = JsonNode.Parse(rawOutput)?.AsObject();
        }
        catch
        {
            return rawOutput;
        }

        if (payload is null)
        {
            return rawOutput;
        }

        var imageBase64 = payload["imageBase64"]?.GetValue<string>();
        if (string.IsNullOrWhiteSpace(imageBase64))
        {
            return rawOutput;
        }

        var mimeType = payload["mimeType"]?.GetValue<string>() ?? "image/png";
        var imageUrl = await imageStore.SaveBase64ImageAsync(imageBase64, mimeType, cancellationToken);

        payload.Remove("imageBase64");
        payload["imageUrl"] = imageUrl;

        return payload.ToJsonString();
    }
}
