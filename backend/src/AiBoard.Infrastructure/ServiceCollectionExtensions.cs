using Amazon;
using Amazon.SQS;
using AiBoard.Application.Abstractions.AI;
using AiBoard.Application.Abstractions.Messaging;
using AiBoard.Application.Abstractions.Persistence;
using AiBoard.Infrastructure.AI;
using AiBoard.Infrastructure.Messaging;
using AiBoard.Infrastructure.Options;
using AiBoard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pgvector.EntityFrameworkCore;

namespace AiBoard.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PersistenceOptions>(configuration.GetSection(PersistenceOptions.SectionName));
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));
        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
        services.Configure<OpenAiOptions>(configuration.GetSection(OpenAiOptions.SectionName));
        services.Configure<StabilityOptions>(configuration.GetSection(StabilityOptions.SectionName));
        services.Configure<AwsOptions>(configuration.GetSection(AwsOptions.SectionName));
        services.Configure<LocalImageOptions>(configuration.GetSection(LocalImageOptions.SectionName));

        var persistence = configuration.GetSection(PersistenceOptions.SectionName).Get<PersistenceOptions>() ?? new PersistenceOptions();
        var ollama = configuration.GetSection(OllamaOptions.SectionName).Get<OllamaOptions>() ?? new OllamaOptions();
        var openAi = configuration.GetSection(OpenAiOptions.SectionName).Get<OpenAiOptions>() ?? new OpenAiOptions();
        var localImage = configuration.GetSection(LocalImageOptions.SectionName).Get<LocalImageOptions>() ?? new LocalImageOptions();

        services.AddDbContext<AiBoardDbContext>(options =>
        {
            if (persistence.Provider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
            {
                options.UseInMemoryDatabase("AiBoardDb");
                return;
            }

            options.UseNpgsql(persistence.ConnectionString, npgsqlOptions => npgsqlOptions.UseVector());
        });

        services.AddHttpClient("ollama", client =>
        {
            client.BaseAddress = new Uri(ollama.BaseUrl);
        });
        services.AddHttpClient("openai", client =>
        {
            client.BaseAddress = new Uri(openAi.BaseUrl);
        });
        services.AddHttpClient("stability", client =>
        {
            // Stability default base; can be overridden via config
            var stability = configuration.GetSection(StabilityOptions.SectionName).Get<StabilityOptions>() ?? new StabilityOptions();
            client.BaseAddress = new Uri(stability.BaseUrl);
        });
        services.AddHttpClient("local-image", client =>
        {
            var baseUrl = localImage.BaseUrl ?? string.Empty;
            if (!baseUrl.EndsWith('/')) baseUrl = baseUrl + "/";
            client.BaseAddress = new Uri(baseUrl);
        });

        services.AddScoped<IAiBoardDbContext>(provider => provider.GetRequiredService<AiBoardDbContext>());
        services.AddSingleton<IJobQueue, RedisJobQueue>();
        services.AddSingleton<StubAiGenerationService>();
        services.AddSingleton<OpenAiImageGenerationService>();
        services.AddSingleton<StabilityImageGenerationService>();
        services.AddSingleton<LocalSdWebUiGenerationService>();
        services.AddSingleton<IImageProviderFactory, ImageProviderFactory>();
        // Register SQS job queue if AWS is enabled
        var awsOptions = configuration.GetSection(AwsOptions.SectionName).Get<AwsOptions>() ?? new AwsOptions();
        if (awsOptions.Enabled && !string.IsNullOrWhiteSpace(awsOptions.SqsQueueUrl))
        {
            // Register IAmazonSQS client manually to avoid dependency on Amazon.Extensions.NETCore.Setup
            services.AddSingleton<Amazon.SQS.IAmazonSQS>(sp =>
            {
                var regionName = string.IsNullOrWhiteSpace(awsOptions.Region) ? "us-east-1" : awsOptions.Region;
                var region = RegionEndpoint.GetBySystemName(regionName);
                return new AmazonSQSClient(region);
            });
            services.AddSingleton<IJobQueue, SqsJobQueue>();
        }
        else
        {
            services.AddSingleton<IAiGenerationService>(provider =>
            {
                var localOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<LocalImageOptions>>().Value;
                if (localOptions.Enabled)
                {
                    return provider.GetRequiredService<LocalSdWebUiGenerationService>();
                }

                var openAiOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OpenAiOptions>>().Value;
                var stabilityOptions = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<StabilityOptions>>().Value;

                if (openAiOptions.Enabled && !string.IsNullOrWhiteSpace(openAiOptions.ApiKey))
                {
                    return provider.GetRequiredService<OpenAiImageGenerationService>();
                }

                if (stabilityOptions.Enabled && !string.IsNullOrWhiteSpace(stabilityOptions.ApiKey))
                {
                    return provider.GetRequiredService<StabilityImageGenerationService>();
                }

                return provider.GetRequiredService<StubAiGenerationService>();
            });
            services.AddSingleton<IJobQueue, RedisJobQueue>();
        }
        
        services.AddScoped<DeterministicEmbeddingService>();
        services.AddScoped<OllamaEmbeddingService>();
        services.AddScoped<IEmbeddingService>(provider =>
        {
            var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OllamaOptions>>().Value;
            return options.Enabled
                ? provider.GetRequiredService<OllamaEmbeddingService>()
                : provider.GetRequiredService<DeterministicEmbeddingService>();
        });
        services.AddScoped<IBoardMemoryService, BoardMemoryService>();
        services.AddScoped<IAssistantOrchestrator, OllamaAssistantOrchestrator>();

        // Configure generated image store: prefer S3 when AWS is enabled and bucket configured
        if (awsOptions.Enabled && !string.IsNullOrWhiteSpace(awsOptions.S3Bucket))
        {
            // Register IAmazonS3 client
            services.AddSingleton<Amazon.S3.IAmazonS3>(sp =>
            {
                var regionName = string.IsNullOrWhiteSpace(awsOptions.Region) ? "us-east-1" : awsOptions.Region;
                var region = RegionEndpoint.GetBySystemName(regionName);
                return new Amazon.S3.AmazonS3Client(region);
            });

            services.AddSingleton<IGeneratedImageStore, S3GeneratedImageStore>();
        }
        else
        {
            services.AddSingleton<IGeneratedImageStore, FileSystemGeneratedImageStore>();
        }

        return services;
    }
}
