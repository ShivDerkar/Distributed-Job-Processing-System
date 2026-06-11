using DistributedJob.Application.Interfaces;
using DistributedJob.Application.Services;
using DistributedJob.Infrastructure.Persistence;
using DistributedJob.Infrastructure.Queue;
using DistributedJob.Infrastructure.Repositories;
using DistributedJob.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using DistributedJob.Application.Factories;
using DistributedJob.Application.JobHandlers;

namespace DistributedJob.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var postgresConnectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<DistributedJobDbContext>(options =>
            options.UseNpgsql(postgresConnectionString));

        var redisConnectionString = configuration.GetConnectionString("Redis");

        if (string.IsNullOrWhiteSpace(redisConnectionString))
        {
            throw new InvalidOperationException("Redis connection string is not configured.");
        }

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(redisConnectionString));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IJobQueue, RedisJobQueue>();
        services.AddScoped<IWorkerNodeRepository, WorkerNodeRepository>();
        services.AddScoped<IWorkerMonitoringService, WorkerMonitoringService>();

        services.AddScoped<IJobHandler, CsvSummaryJobHandler>();
        services.AddScoped<IJobHandler, ReportGenerationJobHandler>();
        services.AddScoped<IJobHandler, EmailNotificationJobHandler>();
        services.AddScoped<IJobHandler, SecurityScanJobHandler>();

        services.AddScoped<IJobHandlerFactory, JobHandlerFactory>();
        return services;
    }
}