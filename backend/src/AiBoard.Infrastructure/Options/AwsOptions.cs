namespace AiBoard.Infrastructure.Options;

public sealed class AwsOptions
{
    public const string SectionName = "Aws";

    public bool Enabled { get; init; }
    public string Region { get; init; } = "us-east-1";
    public string SqsQueueUrl { get; init; } = string.Empty;
    public string S3Bucket { get; init; } = string.Empty;
}
