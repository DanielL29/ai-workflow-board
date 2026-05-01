using AiBoard.Domain.Enums;
using AiBoard.Domain.ValueObjects;

namespace AiBoard.Domain.Entities;

public sealed class BoardNode
{
    private BoardNode()
    {
    }

    public BoardNode(
        Guid boardId,
        NodeType type,
        string title,
        string content,
        NodePosition position,
        string? model = null)
    {
        Id = Guid.NewGuid();
        BoardId = boardId;
        Type = type;
        Title = title;
        Content = content;
        Position = position;
        Model = model;
        Status = NodeExecutionStatus.Idle;
        CreatedAtUtc = DateTime.UtcNow;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public NodeType Type { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public string? Model { get; private set; }
    public NodeExecutionStatus Status { get; private set; }
    public string? OutputContent { get; private set; }
    public NodePosition Position { get; private set; } = new(0, 0);
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    public void Queue()
    {
        Status = NodeExecutionStatus.Queued;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Update(string title, string content, NodePosition position, string? model = null)
    {
        Title = title;
        Content = content;
        Position = position;
        Model = model;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Start()
    {
        Status = NodeExecutionStatus.Running;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Complete(string output)
    {
        Status = NodeExecutionStatus.Completed;
        OutputContent = output;
        UpdatedAtUtc = DateTime.UtcNow;
    }

    public void Fail(string error)
    {
        Status = NodeExecutionStatus.Failed;
        OutputContent = error;
        UpdatedAtUtc = DateTime.UtcNow;
    }
}
