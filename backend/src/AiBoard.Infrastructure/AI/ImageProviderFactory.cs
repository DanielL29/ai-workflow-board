using AiBoard.Application.Abstractions.AI;
using Microsoft.Extensions.Options;
using AiBoard.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;

namespace AiBoard.Infrastructure.AI;

public sealed class ImageProviderFactory : IImageProviderFactory
{
    private readonly IServiceProvider _services;
    private readonly OpenAiOptions _openAiOptions;
    private readonly StabilityOptions _stabilityOptions;

    public ImageProviderFactory(IServiceProvider services,
        IOptions<OpenAiOptions> openAiOptions,
        IOptions<StabilityOptions> stabilityOptions)
    {
        _services = services;
        _openAiOptions = openAiOptions.Value;
        _stabilityOptions = stabilityOptions.Value;
    }

    public IAiGenerationService Get(string? provider = null)
    {
        // provider parameter can be used to force a provider by name
        if (!string.IsNullOrWhiteSpace(provider))
        {
            var trimmed = provider.Trim().ToLowerInvariant();
            if (trimmed == "openai" && _openAiOptions.Enabled && !string.IsNullOrWhiteSpace(_openAiOptions.ApiKey))
            {
                return _services.GetRequiredService<OpenAiImageGenerationService>();
            }

            if (trimmed == "stability" && _stabilityOptions.Enabled && !string.IsNullOrWhiteSpace(_stabilityOptions.ApiKey))
            {
                return _services.GetRequiredService<StabilityImageGenerationService>();
            }
        }

        // Default selection order: OpenAI -> Stability -> Local -> Stub
        if (_openAiOptions.Enabled && !string.IsNullOrWhiteSpace(_openAiOptions.ApiKey))
        {
            return _services.GetRequiredService<OpenAiImageGenerationService>();
        }

        if (_stabilityOptions.Enabled && !string.IsNullOrWhiteSpace(_stabilityOptions.ApiKey))
        {
            return _services.GetRequiredService<StabilityImageGenerationService>();
        }

        var localOptions = _services.GetRequiredService<Microsoft.Extensions.Options.IOptions<LocalImageOptions>>().Value;
        if (localOptions.Enabled)
        {
            return _services.GetRequiredService<LocalSdWebUiGenerationService>();
        }

        return _services.GetRequiredService<StubAiGenerationService>();
    }
}
