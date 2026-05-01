using AiBoard.Application.Abstractions.Messaging;
using AiBoard.Application.Abstractions.Persistence;
using AiBoard.Shared.Contracts;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AiBoard.Application.Jobs.Commands;

public sealed record QueueNodeGenerationCommand(Guid BoardId, Guid NodeId, string Provider, string Prompt) : IRequest<Guid>;

public sealed class QueueNodeGenerationCommandHandler(
    IAiBoardDbContext dbContext,
    IJobQueue jobQueue) : IRequestHandler<QueueNodeGenerationCommand, Guid>
{
    public async Task<Guid> Handle(QueueNodeGenerationCommand request, CancellationToken cancellationToken)
    {
        var node = await dbContext.BoardNodes.FirstOrDefaultAsync(
            x => x.Id == request.NodeId && x.BoardId == request.BoardId,
            cancellationToken) ?? throw new InvalidOperationException("Node not found.");

        node.Queue();

        var job = new Domain.Entities.GenerationJob(request.BoardId, request.NodeId, request.Provider, request.Prompt);
        dbContext.GenerationJobs.Add(job);

        await dbContext.SaveChangesAsync(cancellationToken);

        await jobQueue.EnqueueAsync(
            new NodeGenerationRequested(job.Id, request.BoardId, request.NodeId, request.Provider, request.Prompt),
            cancellationToken);

        return job.Id;
    }
}
