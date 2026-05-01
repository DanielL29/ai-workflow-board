using AiBoard.Application.AI;
using AiBoard.Application.Abstractions.AI;
using AiBoard.Application.Abstractions.Persistence;
using AiBoard.Application.Boards.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiBoard.Application.Boards.Commands;

public sealed record GenerateBoardNodeCommand(Guid BoardId, Guid NodeId, string Provider, string Prompt) : IRequest<BoardNodeDto>;

public sealed class GenerateBoardNodeCommandHandler(
    IAiBoardDbContext dbContext,
    IAiGenerationService generationService,
    IGeneratedImageStore imageStore,
    IBoardMemoryService boardMemoryService) : IRequestHandler<GenerateBoardNodeCommand, BoardNodeDto>
{
    public async Task<BoardNodeDto> Handle(GenerateBoardNodeCommand request, CancellationToken cancellationToken)
    {
        var node = await dbContext.BoardNodes.FirstOrDefaultAsync(
            x => x.Id == request.NodeId && x.BoardId == request.BoardId,
            cancellationToken) ?? throw new InvalidOperationException("Node not found.");

        node.Queue();
        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            node.Start();
            await dbContext.SaveChangesAsync(cancellationToken);

            var rawResult = await generationService.GenerateAsync(request.Provider, request.Prompt, cancellationToken);
            var result = await GeneratedOutputNormalizer.NormalizeAsync(rawResult, imageStore, cancellationToken);
            node.Complete(result);
            await dbContext.SaveChangesAsync(cancellationToken);

            await boardMemoryService.IndexNodeAsync(
                node.BoardId,
                node.Id,
                node.Title,
                node.Content,
                node.OutputContent,
                cancellationToken);
        }
        catch (Exception ex)
        {
            node.Fail(ex.Message);
            await dbContext.SaveChangesAsync(cancellationToken);
            throw;
        }

        return new BoardNodeDto(
            node.Id,
            node.Type,
            node.Title,
            node.Content,
            node.Model,
            node.OutputContent,
            node.Position.X,
            node.Position.Y,
            node.Status);
    }
}
