namespace AiBoard.Api.Contracts.Boards;

public sealed record SearchBoardMemoryRequest(string Query, int Limit = 5);
