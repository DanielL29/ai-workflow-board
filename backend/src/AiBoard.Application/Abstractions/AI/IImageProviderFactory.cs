namespace AiBoard.Application.Abstractions.AI;

public interface IImageProviderFactory
{
    IAiGenerationService Get(string? provider = null);
}
