using AiBoard.Application.Abstractions.AI;
using AiBoard.Application.Boards.Dtos;
using AiBoard.Domain.Entities;
using AiBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace AiBoard.Infrastructure.AI;

public sealed class BoardMemoryService(
    AiBoardDbContext dbContext,
    IEmbeddingService embeddingService) : IBoardMemoryService
{
    private const int DefaultEmbeddingDimensions = 384;

    public async Task<BoardMemoryDocumentDto> UpsertDocumentAsync(
        Guid boardId,
        string sourceType,
        string title,
        string content,
        Guid? sourceNodeId,
        CancellationToken cancellationToken)
    {
        var board = await dbContext.Boards.FirstOrDefaultAsync(x => x.Id == boardId, cancellationToken)
            ?? throw new InvalidOperationException("Board not found.");

        var document = await dbContext.BoardMemoryDocuments
            .Include(x => x.Chunks)
            .FirstOrDefaultAsync(
                x => x.BoardId == boardId && x.SourceNodeId == sourceNodeId && x.SourceType == sourceType,
                cancellationToken);

        if (document is null)
        {
            document = new BoardMemoryDocument(boardId, sourceType, title, content, sourceNodeId);
            dbContext.BoardMemoryDocuments.Add(document);
        }
        else
        {
            document.ReplaceContent(title, content);
            dbContext.BoardMemoryChunks.RemoveRange(document.Chunks);
            document.ClearChunks();
        }

        var chunks = Chunk(content).ToArray();
        var embeddings = await embeddingService.EmbedAsync(chunks, cancellationToken);
        var embeddingList = embeddings.ToArray();

        for (var index = 0; index < chunks.Length; index++)
        {
            var embedding = index < embeddingList.Length
                ? new Vector(embeddingList[index])
                : DeterministicEmbeddingService.ToVector(new float[DefaultEmbeddingDimensions]);

            var chunk = new BoardMemoryChunk(boardId, document.Id, index, chunks[index], embedding);
            document.AddChunk(chunk);
            dbContext.BoardMemoryChunks.Add(chunk);
        }

        board.Touch();
        await dbContext.SaveChangesAsync(cancellationToken);

        return new BoardMemoryDocumentDto(
            document.Id,
            document.BoardId,
            document.SourceNodeId,
            document.SourceType,
            document.Title,
            document.Chunks.Count,
            document.UpdatedAtUtc);
    }

    public async Task<IReadOnlyCollection<BoardMemorySearchResultDto>> SearchAsync(
        Guid boardId,
        string query,
        int limit,
        CancellationToken cancellationToken)
    {
        var normalizedLimit = Math.Clamp(limit, 1, 12);
        var embedding = (await embeddingService.EmbedAsync([query], cancellationToken)).FirstOrDefault()
            ?? DeterministicEmbeddingService.ToVector(new float[DefaultEmbeddingDimensions]).ToArray();

        if (dbContext.Database.IsInMemory())
        {
            var all = await dbContext.BoardMemoryChunks
                .Where(x => x.BoardId == boardId)
                .Join(
                    dbContext.BoardMemoryDocuments,
                    chunk => chunk.DocumentId,
                    document => document.Id,
                    (chunk, document) => new { chunk, document })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return all
                .OrderByDescending(x => CosineSimilarity(x.chunk.Embedding.ToArray(), embedding))
                .Take(normalizedLimit)
                .Select(x => new BoardMemorySearchResultDto(x.document.Id, x.chunk.Id, x.chunk.Sequence, x.document.Title, x.chunk.Content))
                .ToArray();
        }

        var queryVector = new Vector(embedding);
        var rows = await dbContext.BoardMemoryChunks
            .FromSqlInterpolated($@"
                SELECT c.*
                FROM board_memory_chunks c
                WHERE c.""BoardId"" = {boardId}
                ORDER BY c.""Embedding"" <=> {queryVector}
                LIMIT {normalizedLimit}")
            .AsNoTracking()
            .Join(
                dbContext.BoardMemoryDocuments.AsNoTracking(),
                chunk => chunk.DocumentId,
                document => document.Id,
                (chunk, document) => new BoardMemorySearchResultDto(document.Id, chunk.Id, chunk.Sequence, document.Title, chunk.Content))
            .ToArrayAsync(cancellationToken);

        return rows;
    }

    public async Task IndexNodeAsync(
        Guid boardId,
        Guid nodeId,
        string title,
        string? content,
        string? outputContent,
        CancellationToken cancellationToken)
    {
        var normalizedTitle = string.IsNullOrWhiteSpace(title) ? "Untitled node" : title.Trim();

        if (!string.IsNullOrWhiteSpace(content))
        {
            await UpsertDocumentAsync(
                boardId,
                "node-input",
                normalizedTitle,
                content.Trim(),
                nodeId,
                cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(outputContent))
        {
            await UpsertDocumentAsync(
                boardId,
                "node-output",
                $"{normalizedTitle} output",
                outputContent.Trim(),
                nodeId,
                cancellationToken);
        }
    }

    private static IEnumerable<string> Chunk(string content)
    {
        const int chunkSize = 700;
        const int overlap = 120;

        if (string.IsNullOrWhiteSpace(content))
        {
            yield return string.Empty;
            yield break;
        }

        var start = 0;
        while (start < content.Length)
        {
            var length = Math.Min(chunkSize, content.Length - start);
            yield return content.Substring(start, length).Trim();
            if (start + length >= content.Length)
            {
                yield break;
            }

            start += chunkSize - overlap;
        }
    }

    private static double CosineSimilarity(float[] left, float[] right)
    {
        double dot = 0;
        double leftNorm = 0;
        double rightNorm = 0;

        for (var i = 0; i < Math.Min(left.Length, right.Length); i++)
        {
            dot += left[i] * right[i];
            leftNorm += left[i] * left[i];
            rightNorm += right[i] * right[i];
        }

        if (leftNorm == 0 || rightNorm == 0)
        {
            return 0;
        }

        return dot / (Math.Sqrt(leftNorm) * Math.Sqrt(rightNorm));
    }
}
