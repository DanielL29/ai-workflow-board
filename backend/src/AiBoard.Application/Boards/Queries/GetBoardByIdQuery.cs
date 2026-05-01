using AiBoard.Application.Abstractions.Persistence;
using AiBoard.Application.Boards.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiBoard.Application.Boards.Queries;

public sealed record GetBoardByIdQuery(Guid BoardId) : IRequest<BoardDto?>;

public sealed class GetBoardByIdQueryHandler(IAiBoardDbContext dbContext) : IRequestHandler<GetBoardByIdQuery, BoardDto?>
{
    public async Task<BoardDto?> Handle(GetBoardByIdQuery request, CancellationToken cancellationToken)
    {
        var board = await dbContext.Boards
            .Include(x => x.Nodes)
            .Include(x => x.Edges)
            .FirstOrDefaultAsync(x => x.Id == request.BoardId, cancellationToken);

        if (board is null)
        {
            return null;
        }

        var nodes = board.Nodes
            .Select(node => new BoardNodeDto(
                node.Id,
                node.Type,
                node.Title,
                node.Content,
                node.Model,
                node.OutputContent,
                node.Position.X,
                node.Position.Y,
                node.Status))
            .ToArray();

        var edges = board.Edges
            .Select(edge => new BoardEdgeDto(edge.Id, edge.SourceNodeId, edge.TargetNodeId, edge.Label))
            .ToArray();

        return new BoardDto(board.Id, board.Name, board.Description, nodes, edges, board.UpdatedAtUtc);
    }
}
