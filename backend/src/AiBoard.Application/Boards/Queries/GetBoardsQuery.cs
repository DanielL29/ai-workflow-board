using AiBoard.Application.Abstractions.Persistence;
using AiBoard.Application.Boards.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiBoard.Application.Boards.Queries;

public sealed record GetBoardsQuery() : IRequest<BoardDto[]>;

public sealed class GetBoardsQueryHandler(IAiBoardDbContext dbContext) : IRequestHandler<GetBoardsQuery, BoardDto[]>
{
    public async Task<BoardDto[]> Handle(GetBoardsQuery request, CancellationToken cancellationToken)
    {
        var boards = await dbContext.Boards
            .AsNoTracking()
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToArrayAsync(cancellationToken);

        return boards.Select(b => new BoardDto(b.Id, b.Name, b.Description, Array.Empty<BoardNodeDto>(), Array.Empty<BoardEdgeDto>(), b.UpdatedAtUtc)).ToArray();
    }
}
