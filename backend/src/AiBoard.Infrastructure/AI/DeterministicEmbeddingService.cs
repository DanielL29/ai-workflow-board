using System.Security.Cryptography;
using System.Text;
using AiBoard.Application.Abstractions.AI;
using AiBoard.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Pgvector;

namespace AiBoard.Infrastructure.AI;

public sealed class DeterministicEmbeddingService(IOptions<OllamaOptions> options) : IEmbeddingService
{
    private readonly int _dimensions = Math.Max(8, options.Value.EmbeddingDimensions);

    public Task<IReadOnlyCollection<float[]>> EmbedAsync(IReadOnlyCollection<string> inputs, CancellationToken cancellationToken)
    {
        var result = inputs.Select(EmbedSingle).ToArray();
        return Task.FromResult<IReadOnlyCollection<float[]>>(result);
    }

    private float[] EmbedSingle(string input)
    {
        var vector = new float[_dimensions];
        var tokens = input.Split([' ', '\r', '\n', '\t', ',', '.', ';', ':', '!', '?'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (tokens.Length == 0)
        {
            return vector;
        }

        foreach (var token in tokens)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token.ToLowerInvariant()));
            for (var i = 0; i < bytes.Length; i += 4)
            {
                var slot = BitConverter.ToInt32(bytes, i) % _dimensions;
                if (slot < 0)
                {
                    slot += _dimensions;
                }

                vector[slot] += 1f;
            }
        }

        return Normalize(vector);
    }

    public static Vector ToVector(float[] values) => new(Normalize(values));

    private static float[] Normalize(float[] values)
    {
        var magnitude = Math.Sqrt(values.Sum(x => x * x));
        if (magnitude <= 0)
        {
            return values;
        }

        for (var i = 0; i < values.Length; i++)
        {
            values[i] = (float)(values[i] / magnitude);
        }

        return values;
    }
}
