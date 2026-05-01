using AiBoard.Domain.Enums;

namespace AiBoard.Api.Contracts.Boards;

public sealed record CreateBoardNodeRequest(
    NodeType Type,
    string Title,
    string Content,
    decimal X,
    decimal Y,
    string? Model);
