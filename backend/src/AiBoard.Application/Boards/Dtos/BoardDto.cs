namespace AiBoard.Application.Boards.Dtos;

public sealed record BoardDto(
    Guid Id,
    string Name,
    string? Description,
    IReadOnlyCollection<BoardNodeDto> Nodes,
    IReadOnlyCollection<BoardEdgeDto> Edges,
    DateTime UpdatedAtUtc);
