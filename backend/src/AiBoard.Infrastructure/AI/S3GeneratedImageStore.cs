using AiBoard.Application.Abstractions.AI;
using AiBoard.Infrastructure.Options;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;

namespace AiBoard.Infrastructure.AI;

public sealed class S3GeneratedImageStore : IGeneratedImageStore
{
    private readonly IAmazonS3 _s3;
    private readonly AwsOptions _options;

    public S3GeneratedImageStore(IAmazonS3 s3, IOptions<AwsOptions> options)
    {
        _s3 = s3;
        _options = options.Value;
    }

    public async Task<string> SaveBase64ImageAsync(string imageBase64, string mimeType, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.S3Bucket))
        {
            throw new InvalidOperationException("S3 bucket is not configured in AwsOptions.");
        }

        var extension = mimeType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/webp" => ".webp",
            _ => ".png"
        };

        var key = $"generated-images/{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}{extension}";
        var bytes = Convert.FromBase64String(imageBase64);

        using var stream = new MemoryStream(bytes);
        var put = new PutObjectRequest
        {
            BucketName = _options.S3Bucket,
            Key = key,
            InputStream = stream,
            ContentType = mimeType,
            AutoCloseStream = true
        };

        await _s3.PutObjectAsync(put, cancellationToken);

        // Return a public S3 URL. Assumes the bucket/objects are publicly readable or fronted by CDN.
        var region = string.IsNullOrWhiteSpace(_options.Region) ? "us-east-1" : _options.Region;
        var url = region == "us-east-1"
            ? $"https://{_options.S3Bucket}.s3.amazonaws.com/{key}"
            : $"https://{_options.S3Bucket}.s3.{region}.amazonaws.com/{key}";

        return url;
    }
}
