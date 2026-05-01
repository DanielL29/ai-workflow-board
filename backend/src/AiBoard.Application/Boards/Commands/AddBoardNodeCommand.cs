using AiBoard.Application.Abstractions.AI;
using AiBoard.Application.Abstractions.Persistence;
using AiBoard.Application.Boards.Dtos;
using AiBoard.Domain.Entities;
using AiBoard.Domain.Enums;
using AiBoard.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiBoard.Application.Boards.Commands;

public sealed record AddBoardNodeCommand(
    Guid BoardId,
    NodeType Type,
    string Title,
    string Content,
    decimal X,
    decimal Y,
    string? Model) : IRequest<BoardNodeDto>;

public sealed class AddBoardNodeCommandHandler(
    IAiBoardDbContext dbContext,
    IBoardMemoryService boardMemoryService) : IRequestHandler<AddBoardNodeCommand, BoardNodeDto>
{
    public async Task<BoardNodeDto> Handle(AddBoardNodeCommand request, CancellationToken cancellationToken)
    {
        var board = await dbContext.Boards.FirstOrDefaultAsync(x => x.Id == request.BoardId, cancellationToken)
            ?? throw new InvalidOperationException("Board not found.");

        var node = new BoardNode(
            request.BoardId,
            request.Type,
            request.Title,
            request.Content,
            new NodePosition(request.X, request.Y),
            request.Model);

        board.AddNode(node);
        dbContext.BoardNodes.Add(node);
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
