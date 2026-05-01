namespace AiBoard.Application.Boards.Dtos;

public sealed record BoardMemorySearchResultDto(
    Guid DocumentId,
    Guid ChunkId,
    int Sequence,
    string Title,
    string Content);
