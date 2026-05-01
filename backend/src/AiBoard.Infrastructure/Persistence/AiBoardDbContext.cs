using AiBoard.Application.Abstractions.Persistence;
using AiBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;

namespace AiBoard.Infrastructure.Persistence;

public sealed class AiBoardDbContext(DbContextOptions<AiBoardDbContext> options) : DbContext(options), IAiBoardDbContext
{
    public DbSet<Board> Boards => Set<Board>();
    public DbSet<BoardNode> BoardNodes => Set<BoardNode>();
    public DbSet<BoardEdge> BoardEdges => Set<BoardEdge>();
    public DbSet<GenerationJob> GenerationJobs => Set<GenerationJob>();
    public DbSet<BoardMemoryDocument> BoardMemoryDocuments => Set<BoardMemoryDocument>();
    public DbSet<BoardMemoryChunk> BoardMemoryChunks => Set<BoardMemoryChunk>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("vector");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AiBoardDbContext).Assembly);
    }
}
