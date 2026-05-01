namespace AiBoard.Infrastructure.Options;

public sealed class StabilityOptions
{
    public const string SectionName = "Stability";

    public bool Enabled { get; init; }
    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = "https://api.stability.ai/";
    public string Engine { get; init; } = "stable-diffusion-xl-1024-v1-0";
    public int Width { get; init; } = 1024;
    public int Height { get; init; } = 1024;
    public float Sampler { get; init; } = 1.0f;
}
