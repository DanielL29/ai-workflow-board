namespace AiBoard.Api.Contracts.Boards;

public sealed record QueueGenerationRequest(Guid NodeId, string Provider, string Prompt);
