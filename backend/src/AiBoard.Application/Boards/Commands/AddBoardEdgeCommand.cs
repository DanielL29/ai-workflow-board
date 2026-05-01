using AiBoard.Application.Abstractions.Persistence;
using AiBoard.Application.Boards.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiBoard.Application.Boards.Commands;

public sealed record AddBoardEdgeCommand(Guid BoardId, Guid SourceNodeId, Guid TargetNodeId, string? Label) : IRequest<BoardEdgeDto>;

public sealed class AddBoardEdgeCommandHandler(IAiBoardDbContext dbContext) : IRequestHandler<AddBoardEdgeCommand, BoardEdgeDto>
{
    public async Task<BoardEdgeDto> Handle(AddBoardEdgeCommand request, CancellationToken cancellationToken)
    {
        var board = await dbContext.Boards
            .Include(x => x.Nodes)
            .FirstOrDefaultAsync(x => x.Id == request.BoardId, cancellationToken)
            ?? throw new InvalidOperationException("Board not found.");

        if (!board.Nodes.Any(x => x.Id == request.SourceNodeId) || !board.Nodes.Any(x => x.Id == request.TargetNodeId))
        {
            throw new InvalidOperationException("Source or target node does not belong to the board.");
        }

        var edge = board.AddEdge(request.SourceNodeId, request.TargetNodeId, request.Label);
        dbContext.BoardEdges.Add(edge);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new BoardEdgeDto(edge.Id, edge.SourceNodeId, edge.TargetNodeId, edge.Label);
    }
}
