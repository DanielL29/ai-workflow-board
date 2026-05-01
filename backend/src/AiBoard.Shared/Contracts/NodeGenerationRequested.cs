namespace AiBoard.Shared.Contracts;

public sealed record NodeGenerationRequested(
    Guid JobId,
    Guid BoardId,
    Guid NodeId,
    string Provider,
    string Prompt);
