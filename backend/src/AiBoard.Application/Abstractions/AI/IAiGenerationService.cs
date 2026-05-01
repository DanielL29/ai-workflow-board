namespace AiBoard.Application.Abstractions.AI;

public interface IAiGenerationService
{
    Task<string> GenerateAsync(string provider, string prompt, CancellationToken cancellationToken);
}
