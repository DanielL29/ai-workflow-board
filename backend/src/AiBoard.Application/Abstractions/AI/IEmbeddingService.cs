namespace AiBoard.Application.Abstractions.AI;

public interface IEmbeddingService
{
    Task<IReadOnlyCollection<float[]>> EmbedAsync(IReadOnlyCollection<string> inputs, CancellationToken cancellationToken);
}
