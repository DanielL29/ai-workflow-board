using AiBoard.Application.Boards.Dtos;

namespace AiBoard.Application.Abstractions.AI;

public interface IBoardMemoryService
{
    Task<BoardMemoryDocumentDto> UpsertDocumentAsync(
        Guid boardId,
        string sourceType,
        string title,
        string content,
        Guid? sourceNodeId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<BoardMemorySearchResultDto>> SearchAsync(
        Guid boardId,
        string query,
        int limit,
        CancellationToken cancellationToken);

    Task IndexNodeAsync(
        Guid boardId,
        Guid nodeId,
        string title,
        string? content,
        string? outputContent,
        CancellationToken cancellationToken);
}
