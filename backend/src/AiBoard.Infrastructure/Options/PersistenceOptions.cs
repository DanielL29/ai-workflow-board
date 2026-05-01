namespace AiBoard.Infrastructure.Options;

public sealed class PersistenceOptions
{
    public const string SectionName = "Persistence";
    public string Provider { get; set; } = "Postgres";
    public string ConnectionString { get; set; } = "Host=postgres;Port=5432;Database=aiboard;Username=postgres;Password=postgres";
}
