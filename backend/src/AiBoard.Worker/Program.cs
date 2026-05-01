using AiBoard.Infrastructure;
using AiBoard.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<GenerationWorker>();

var host = builder.Build();
host.Run();
