using DistributedJob.Application.Interfaces;
using DistributedJob.Application.Services;
using DistributedJob.Infrastructure.Persistence;
using DistributedJob.Infrastructure.Repositories;
using DistributedJob.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedJob.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<DistributedJobDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IJobService, JobService>();

        return services;
    }
}