using AiBoard.Application.Abstractions.AI;
using AiBoard.Application.Abstractions.Persistence;
using AiBoard.Application.Boards.Dtos;
using AiBoard.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiBoard.Application.Boards.Commands;

public sealed record UpdateBoardNodeCommand(
    Guid BoardId,
    Guid NodeId,
    string Title,
    string Content,
    decimal X,
    decimal Y,
    string? Model) : IRequest<BoardNodeDto>;

public sealed class UpdateBoardNodeCommandHandler(
    IAiBoardDbContext dbContext,
    IBoardMemoryService boardMemoryService) : IRequestHandler<UpdateBoardNodeCommand, BoardNodeDto>
{
    public async Task<BoardNodeDto> Handle(UpdateBoardNodeCommand request, CancellationToken cancellationToken)
    {
        var node = await dbContext.BoardNodes.FirstOrDefaultAsync(
            x => x.Id == request.NodeId && x.BoardId == request.BoardId,
            cancellationToken) ?? throw new InvalidOperationException("Node not found.");

        node.Update(
            request.Title,
            request.Content,
            new NodePosition(request.X, request.Y),
            request.Model);

        await dbContext.SaveChangesAsync(cancellationToken);

        await boardMemoryService.IndexNodeAsync(
            node.BoardId,
            node.Id,
            node.Title,
            node.Content,
            node.OutputContent,
            cancellationToken);

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
