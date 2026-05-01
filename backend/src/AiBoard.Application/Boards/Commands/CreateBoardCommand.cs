using AiBoard.Application.Abstractions.Persistence;
using AiBoard.Application.Boards.Dtos;
using AiBoard.Domain.Entities;
using MediatR;

namespace AiBoard.Application.Boards.Commands;

public sealed record CreateBoardCommand(string Name, string? Description) : IRequest<BoardDto>;

public sealed class CreateBoardCommandHandler(IAiBoardDbContext dbContext) : IRequestHandler<CreateBoardCommand, BoardDto>
{
    public async Task<BoardDto> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
    {
        var board = new Board(request.Name, request.Description);
        dbContext.Boards.Add(board);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new BoardDto(board.Id, board.Name, board.Description, [], [], board.UpdatedAtUtc);
    }
}
