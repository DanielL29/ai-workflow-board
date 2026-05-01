namespace AiBoard.Application.Boards.Dtos;

public sealed record BoardMemoryDocumentDto(
    Guid Id,
    Guid BoardId,
    Guid? SourceNodeId,
    string SourceType,
    string Title,
    int ChunkCount,
    DateTime UpdatedAtUtc);
