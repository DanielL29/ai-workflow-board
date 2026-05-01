namespace AiBoard.Application.Abstractions.AI;

public interface IAssistantOrchestrator
{
    Task<string> ReplyAsync(Guid? boardId, string message, CancellationToken cancellationToken);
}
