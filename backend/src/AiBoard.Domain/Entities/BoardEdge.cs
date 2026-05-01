namespace AiBoard.Domain.Entities;

public sealed class BoardEdge
{
    private BoardEdge()
    {
    }

    public BoardEdge(Guid boardId, Guid sourceNodeId, Guid targetNodeId, string? label = null)
    {
        Id = Guid.NewGuid();
        BoardId = boardId;
        SourceNodeId = sourceNodeId;
        TargetNodeId = targetNodeId;
        Label = label;
    }

    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public Guid SourceNodeId { get; private set; }
    public Guid TargetNodeId { get; private set; }
    public string? Label { get; private set; }
}
