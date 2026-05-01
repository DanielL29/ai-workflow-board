namespace AiBoard.Infrastructure.Options;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";
    public string ConnectionString { get; set; } = "redis:6379";
}
