using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Pgvector.EntityFrameworkCore;

namespace AiBoard.Infrastructure.Persistence;

public sealed class AiBoardDbContextFactory : IDesignTimeDbContextFactory<AiBoardDbContext>
{
    public AiBoardDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AiBoardDbContext>();
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=aiboard;Username=postgres;Password=postgres",
            npgsqlOptions => npgsqlOptions.UseVector());

        return new AiBoardDbContext(optionsBuilder.Options);
    }
}
