using AiBoard.Shared.Contracts;

namespace AiBoard.Application.Abstractions.Messaging;

public interface IJobQueue
{
    Task EnqueueAsync(NodeGenerationRequested message, CancellationToken cancellationToken);
    Task<NodeGenerationRequested?> DequeueAsync(CancellationToken cancellationToken);
}
