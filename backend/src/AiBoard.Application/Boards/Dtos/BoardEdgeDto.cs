namespace AiBoard.Application.Boards.Dtos;

public sealed record BoardEdgeDto(
    Guid Id,
    Guid SourceNodeId,
    Guid TargetNodeId,
    string? Label);
