using System.Text;
using System.Text.Json;
using AiBoard.Application.Abstractions.Messaging;
using AiBoard.Shared.Contracts;
using AiBoard.Infrastructure.Options;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;

namespace AiBoard.Infrastructure.Messaging;

public sealed class SqsJobQueue : IJobQueue
{
    private readonly IAmazonSQS _sqs;
    private readonly AwsOptions _options;

    public SqsJobQueue(IAmazonSQS sqs, IOptions<AwsOptions> options)
    {
        _sqs = sqs;
        _options = options.Value;
    }

    public async Task EnqueueAsync(NodeGenerationRequested message, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.Serialize(message);
        var req = new SendMessageRequest
        {
            QueueUrl = _options.SqsQueueUrl,
            MessageBody = payload
        };

        await _sqs.SendMessageAsync(req, cancellationToken);
    }

    public async Task<NodeGenerationRequested?> DequeueAsync(CancellationToken cancellationToken)
    {
        var req = new ReceiveMessageRequest
        {
            QueueUrl = _options.SqsQueueUrl,
            MaxNumberOfMessages = 1,
            WaitTimeSeconds = 20
        };

        var resp = await _sqs.ReceiveMessageAsync(req, cancellationToken);
        var msg = resp.Messages.FirstOrDefault();
        if (msg is null) return null;

        try
        {
            var body = msg.Body;
            var job = JsonSerializer.Deserialize<NodeGenerationRequested>(body);
            // Delete the message after deserializing
            await _sqs.DeleteMessageAsync(_options.SqsQueueUrl, msg.ReceiptHandle, cancellationToken);
            return job;
        }
        catch
        {
            // If deserialization fails, delete to avoid poison messages
            await _sqs.DeleteMessageAsync(_options.SqsQueueUrl, msg.ReceiptHandle, cancellationToken);
            return null;
        }
    }
}
