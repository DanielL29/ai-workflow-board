using System.Text.Json;
using AiBoard.Application.Abstractions.Messaging;
using AiBoard.Infrastructure.Options;
using AiBoard.Shared.Contracts;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace AiBoard.Infrastructure.Messaging;

public sealed class RedisJobQueue : IJobQueue, IDisposable
{
    private readonly ConnectionMultiplexer _connection;
    private readonly IDatabase _db;
    private readonly string _listKey = "aiboard:jobqueue";

    public RedisJobQueue(IOptions<RedisOptions> options)
    {
        var connStr = options?.Value?.ConnectionString ?? "redis:6379";
        _connection = ConnectionMultiplexer.Connect(connStr);
        _db = _connection.GetDatabase();
    }

    public async Task EnqueueAsync(NodeGenerationRequested message, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(message);
        await _db.ListLeftPushAsync(_listKey, json).ConfigureAwait(false);
    }

    public async Task<NodeGenerationRequested?> DequeueAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var val = await _db.ListRightPopAsync(_listKey).ConfigureAwait(false);
            if (val.HasValue)
            {
                try
                {
                    var message = JsonSerializer.Deserialize<NodeGenerationRequested>(val!);
                    return message;
                }
                catch
                {
                    // ignore malformed entry
                }
            }

            await Task.Delay(500, cancellationToken).ConfigureAwait(false);
        }

        return null;
    }

    public void Dispose()
    {
        try
        {
            _connection?.Dispose();
        }
        catch
        {
        }
    }
}
