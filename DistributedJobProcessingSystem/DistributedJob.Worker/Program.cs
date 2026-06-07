using DistributedJob.Infrastructure.Extensions;
using DistributedJob.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<JobWorker>();

var host = builder.Build();

host.Run();