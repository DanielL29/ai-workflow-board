namespace AiBoard.Domain.Entities;

public sealed class BoardMemoryDocument
{
    private readonly List<BoardMemoryChunk> _chunks = [];

    private BoardMemoryDocument()
    {
    }

    public BoardMemoryDocument(
        Guid boardId,
        string sourceType,
        string title,
        string content,
        Guid? sourceNodeId = null)
    {
        Id = Guid.NewGuid();
        BoardId = boardId;
        SourceType = sourceType;
        SourceNodeId = sourceNodeId;
        Title = title;
        Content = content;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public Guid? SourceNodeId { get; private set; }
    public string SourceType { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<BoardMemoryChunk> Chunks => _chunks;

    public void ReplaceContent(string title, string content)
    {
        Title = title;
        Content = content;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void ClearChunks()
    {
        _chunks.Clear();
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void AddChunk(BoardMemoryChunk chunk)
    {
        _chunks.Add(chunk);
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
