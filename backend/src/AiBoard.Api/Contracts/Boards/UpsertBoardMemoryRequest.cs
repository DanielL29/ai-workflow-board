namespace AiBoard.Api.Contracts.Boards;

public sealed record UpsertBoardMemoryRequest(
    string SourceType,
    string Title,
    string Content,
    Guid? SourceNodeId);
