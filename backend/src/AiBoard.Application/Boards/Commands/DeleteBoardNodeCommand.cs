using AiBoard.Application.Abstractions.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiBoard.Application.Boards.Commands;

public sealed record DeleteBoardNodeCommand(Guid BoardId, Guid NodeId) : IRequest<Unit>;

public sealed class DeleteBoardNodeCommandHandler(IAiBoardDbContext dbContext) : IRequestHandler<DeleteBoardNodeCommand, Unit>
{
    public async Task<Unit> Handle(DeleteBoardNodeCommand request, CancellationToken cancellationToken)
    {
        var node = await dbContext.BoardNodes.FirstOrDefaultAsync(x => x.Id == request.NodeId && x.BoardId == request.BoardId, cancellationToken);
        if (node is null)
        {
            return Unit.Value;
        }

        dbContext.BoardNodes.Remove(node);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
