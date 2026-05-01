namespace AiBoard.Infrastructure.Options;

public sealed class OllamaOptions
{
    public const string SectionName = "Ollama";
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string ChatModel { get; set; } = "qwen3:4b";
    public string EmbeddingModel { get; set; } = "embeddinggemma";
    public int EmbeddingDimensions { get; set; } = 384;
}
