namespace AiBoard.Application.Abstractions.AI;

public interface IGeneratedImageStore
{
    Task<string> SaveBase64ImageAsync(string imageBase64, string mimeType, CancellationToken cancellationToken);
}
