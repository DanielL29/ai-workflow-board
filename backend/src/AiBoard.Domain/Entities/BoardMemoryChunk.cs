using Pgvector;

namespace AiBoard.Domain.Entities;

public sealed class BoardMemoryChunk
{
    private BoardMemoryChunk()
    {
    }

    public BoardMemoryChunk(
        Guid boardId,
        Guid documentId,
        int sequence,
        string content,
        Vector embedding)
    {
        Id = Guid.NewGuid();
        BoardId = boardId;
        DocumentId = documentId;
        Sequence = sequence;
        Content = content;
        Embedding = embedding;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public Guid DocumentId { get; private set; }
    public int Sequence { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public Vector Embedding { get; private set; } = new(Array.Empty<float>());
    public DateTime CreatedAtUtc { get; private set; }
}
