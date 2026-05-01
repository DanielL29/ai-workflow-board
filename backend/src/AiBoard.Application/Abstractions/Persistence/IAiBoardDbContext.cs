using AiBoard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AiBoard.Application.Abstractions.Persistence;

public interface IAiBoardDbContext
{
    DbSet<Board> Boards { get; }
    DbSet<BoardNode> BoardNodes { get; }
    DbSet<BoardEdge> BoardEdges { get; }
    DbSet<GenerationJob> GenerationJobs { get; }
    DbSet<BoardMemoryDocument> BoardMemoryDocuments { get; }
    DbSet<BoardMemoryChunk> BoardMemoryChunks { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
