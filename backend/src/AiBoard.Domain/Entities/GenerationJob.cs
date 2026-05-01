using AiBoard.Domain.Enums;

namespace AiBoard.Domain.Entities;

public sealed class GenerationJob
{
    private GenerationJob()
    {
    }

    public GenerationJob(Guid boardId, Guid nodeId, string provider, string prompt)
    {
        Id = Guid.NewGuid();
        BoardId = boardId;
        NodeId = nodeId;
        Provider = provider;
        Prompt = prompt;
        Status = JobStatus.Pending;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public Guid NodeId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string Prompt { get; private set; } = string.Empty;
    public JobStatus Status { get; private set; }
    public string? ResultPayload { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ProcessedAtUtc { get; private set; }

    public void MarkProcessing()
    {
        Status = JobStatus.Processing;
    }

    public void MarkSucceeded(string payload)
    {
        Status = JobStatus.Succeeded;
        ResultPayload = payload;
        ProcessedAtUtc = DateTime.UtcNow;
    }

    public void MarkFailed(string error)
    {
        Status = JobStatus.Failed;
        ErrorMessage = error;
        ProcessedAtUtc = DateTime.UtcNow;
    }
}
