namespace AiBoard.Api.Contracts.Boards;

public sealed record AssistantPromptRequest(Guid? BoardId, string Message);
