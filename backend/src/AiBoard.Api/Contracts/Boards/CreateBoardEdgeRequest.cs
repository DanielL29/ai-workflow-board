namespace AiBoard.Api.Contracts.Boards;

public sealed record CreateBoardEdgeRequest(Guid SourceNodeId, Guid TargetNodeId, string? Label);
