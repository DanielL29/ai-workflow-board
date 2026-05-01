using AiBoard.Domain.Enums;

namespace AiBoard.Application.Boards.Dtos;

public sealed record BoardNodeDto(
    Guid Id,
    NodeType Type,
    string Title,
    string Content,
    string? Model,
    string? OutputContent,
    decimal X,
    decimal Y,
    NodeExecutionStatus Status);
