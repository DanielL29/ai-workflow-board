namespace AiBoard.Api.Contracts.Boards;

public sealed record UpdateBoardNodeRequest(
    string Title,
    string Content,
    decimal X,
    decimal Y,
    string? Model);
