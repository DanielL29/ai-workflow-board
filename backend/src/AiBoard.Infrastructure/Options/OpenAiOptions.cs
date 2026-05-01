namespace AiBoard.Infrastructure.Options;

public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    public bool Enabled { get; init; }
    public string ApiKey { get; init; } = string.Empty;
    public string BaseUrl { get; init; } = "https://api.openai.com/v1/";
    public string ImageModel { get; init; } = "gpt-image-2";
    public string Size { get; init; } = "1024x1024";
    public string Quality { get; init; } = "medium";
    public string Background { get; init; } = "auto";
    public string OutputFormat { get; init; } = "png";
}
