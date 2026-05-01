using AiBoard.Application.AI;
using AiBoard.Application.Abstractions.AI;
using AiBoard.Application.Abstractions.Messaging;
using AiBoard.Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR.Client;


namespace AiBoard.Worker.Services;

public sealed class GenerationWorker(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<GenerationWorker> logger) : BackgroundService
{
    private HubConnection? _hubConnection;
    private readonly TimeSpan _initialRetry = TimeSpan.FromSeconds(2);
    private readonly TimeSpan _maxRetry = TimeSpan.FromSeconds(30);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var apiUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://api:8080/hubs/board";
        async Task EnsureHubConnectedAsync(CancellationToken ct)
        {
            if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected)
                return;

            var attempt = 0;
            var delay = _initialRetry;

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (_hubConnection is null)
                    {
                        _hubConnection = new HubConnectionBuilder().WithUrl(apiUrl).WithAutomaticReconnect().Build();
                    }

                    if (_hubConnection.State != HubConnectionState.Connected)
                    {
                        await _hubConnection.StartAsync(ct);
                        logger.LogInformation("Worker connected to API hub at {ApiUrl}", apiUrl);
                    }

                    return;
                }
                catch (Exception ex)
                {
                    attempt++;
                    logger.LogWarning(ex, "Could not connect worker to API hub at {ApiUrl} (attempt {Attempt}). Retrying in {Delay}s", apiUrl, attempt, delay.TotalSeconds);
                    try
                    {
                        await Task.Delay(delay, ct);
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested)
                    {
                        return;
                    }

                    delay = TimeSpan.FromSeconds(Math.Min(_maxRetry.TotalSeconds, delay.TotalSeconds * 2));
                }
            }
        }
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var jobQueue = scope.ServiceProvider.GetRequiredService<IJobQueue>();
            var dbContext = scope.ServiceProvider.GetRequiredService<IAiBoardDbContext>();
            var generationService = scope.ServiceProvider.GetRequiredService<IAiGenerationService>();
            var imageStore = scope.ServiceProvider.GetRequiredService<IGeneratedImageStore>();
            var boardMemoryService = scope.ServiceProvider.GetRequiredService<IBoardMemoryService>();

            var message = await jobQueue.DequeueAsync(stoppingToken);
            if (message is null)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                continue;
            }

            // Try to ensure SignalR connection is available before processing the job
            await EnsureHubConnectedAsync(stoppingToken);

            var job = await dbContext.GenerationJobs.FirstOrDefaultAsync(x => x.Id == message.JobId, stoppingToken);
            var node = await dbContext.BoardNodes.FirstOrDefaultAsync(x => x.Id == message.NodeId, stoppingToken);

            if (job is null || node is null)
            {
                logger.LogWarning("Skipped job {JobId} because the database record was not found.", message.JobId);
                continue;
            }

            try
            {
                job.MarkProcessing();
                node.Start();
                await dbContext.SaveChangesAsync(stoppingToken);

                var rawResult = await generationService.GenerateAsync(message.Provider, message.Prompt, stoppingToken);
                var result = await GeneratedOutputNormalizer.NormalizeAsync(rawResult, imageStore, stoppingToken);

                job.MarkSucceeded(result);
                node.Complete(result);
                await dbContext.SaveChangesAsync(stoppingToken);
                await boardMemoryService.IndexNodeAsync(
                    node.BoardId,
                    node.Id,
                    node.Title,
                    node.Content,
                    node.OutputContent,
                    stoppingToken);

                    try
                    {
                        if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected)
                        {
                            await _hubConnection.InvokeAsync("NotifyNodeUpdated", node.BoardId.ToString(), new
                            {
                                nodeId = node.Id,
                                status = node.Status,
                                output = node.OutputContent,
                            }, cancellationToken: stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to notify hub about node update for {NodeId}", node.Id);
                    }
            }
            catch (Exception ex)
            {
                job.MarkFailed(ex.Message);
                node.Fail(ex.Message);
                await dbContext.SaveChangesAsync(stoppingToken);
                logger.LogError(ex, "Failed to process generation job {JobId}.", message.JobId);
            }
        }
    }
}
