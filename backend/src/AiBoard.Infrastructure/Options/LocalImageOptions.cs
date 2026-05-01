namespace AiBoard.Infrastructure.Options;

public sealed class LocalImageOptions
{
    public const string SectionName = "LocalImage";

    public bool Enabled { get; init; } = true;
    public string BaseUrl { get; init; } = "http://127.0.0.1:7860/";
    public string Txt2ImgPath { get; init; } = "sdapi/v1/txt2img";
    public int Steps { get; init; } = 20;
    public int Width { get; init; } = 1024;
    public int Height { get; init; } = 1024;
    public int CfgScale { get; init; } = 7;
    public string SamplerName { get; init; } = "Euler a";
    public string NegativePrompt { get; init; } = "";
}
