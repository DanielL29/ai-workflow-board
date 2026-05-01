using AiBoard.Domain.ValueObjects;

namespace AiBoard.Domain.Entities;

public sealed class Board
{
    private readonly List<BoardNode> _nodes = [];
    private readonly List<BoardEdge> _edges = [];
    private readonly List<BoardMemoryDocument> _memoryDocuments = [];

    private Board()
    {
    }

    public Board(string name, string? description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }
    public IReadOnlyCollection<BoardNode> Nodes => _nodes;
    public IReadOnlyCollection<BoardEdge> Edges => _edges;
    public IReadOnlyCollection<BoardMemoryDocument> MemoryDocuments => _memoryDocuments;

    public BoardNode AddNode(BoardNode node)
    {
        _nodes.Add(node);
        Touch();
        return node;
    }

    public BoardEdge AddEdge(Guid sourceNodeId, Guid targetNodeId, string? label = null)
    {
        var edge = new BoardEdge(Id, sourceNodeId, targetNodeId, label);
        _edges.Add(edge);
        Touch();
        return edge;
    }

    public void Touch()
    {
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
