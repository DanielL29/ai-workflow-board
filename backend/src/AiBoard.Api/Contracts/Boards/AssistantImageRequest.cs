namespace AiBoard.Api.Contracts.Boards;

public sealed record AssistantImageRequest(Guid? BoardId, string Prompt, string? Provider);
