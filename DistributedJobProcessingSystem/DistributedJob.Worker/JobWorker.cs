using DistributedJob.Application.Interfaces;
using DistributedJob.Domain.Entities;
using DistributedJob.Domain.Enums;
using DistributedJob.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DistributedJob.Worker;

public class JobWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobWorker> _logger;
    private readonly IConfiguration _configuration;

    public JobWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<JobWorker> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var workerName = _configuration["Worker:Name"] ?? "Worker-1";
        var pollingDelaySeconds = int.Parse(
            _configuration["Worker:PollingDelaySeconds"] ?? "3");

        _logger.LogInformation("{WorkerName} started.", workerName);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var jobQueue = scope.ServiceProvider.GetRequiredService<IJobQueue>();
                var dbContext = scope.ServiceProvider.GetRequiredService<DistributedJobDbContext>();

                var jobId = await jobQueue.DequeueAsync(stoppingToken);

                if (jobId is null)
                {
                    _logger.LogInformation("{WorkerName} found no pending jobs.", workerName);

                    await Task.Delay(
                        TimeSpan.FromSeconds(pollingDelaySeconds),
                        stoppingToken);

                    continue;
                }

                _logger.LogInformation(
                    "{WorkerName} picked job {JobId} from Redis.",
                    workerName,
                    jobId);

                var job = await dbContext.Jobs
                    .FirstOrDefaultAsync(x => x.Id == jobId.Value, stoppingToken);

                if (job is null)
                {
                    _logger.LogWarning(
                        "Job {JobId} was found in Redis but not in PostgreSQL.",
                        jobId);

                    continue;
                }

                if (job.Status == JobStatus.Cancelled)
                {
                    await AddJobLogAsync(
                        dbContext,
                        job.Id,
                        $"{workerName} skipped job because it was already cancelled.",
                        "Warning",
                        stoppingToken);

                    continue;
                }

                await MarkJobRunningAsync(
                    dbContext,
                    job.Id,
                    workerName,
                    stoppingToken);

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

                await MarkJobSucceededAsync(
                    dbContext,
                    job.Id,
                    job.Type,
                    job.InputPayload,
                    workerName,
                    stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unexpected error occurred in worker loop.");

                await Task.Delay(
                    TimeSpan.FromSeconds(pollingDelaySeconds),
                    stoppingToken);
            }
        }

        _logger.LogInformation("{WorkerName} stopped.", workerName);
    }

    private static async Task MarkJobRunningAsync(
        DistributedJobDbContext dbContext,
        Guid jobId,
        string workerName,
        CancellationToken cancellationToken)
    {
        var job = await dbContext.Jobs
            .FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken);

        if (job is null)
        {
            return;
        }

        job.Status = JobStatus.Running;
        job.StartedAt = DateTime.UtcNow;

        dbContext.JobLogs.Add(new JobLog
        {
            JobId = jobId,
            Message = $"{workerName} started processing job.",
            Level = "Info",
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task MarkJobSucceededAsync(
        DistributedJobDbContext dbContext,
        Guid jobId,
        JobType jobType,
        string inputPayload,
        string workerName,
        CancellationToken cancellationToken)
    {
        var job = await dbContext.Jobs
            .FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken);

        if (job is null)
        {
            return;
        }

        job.Status = JobStatus.Succeeded;
        job.CompletedAt = DateTime.UtcNow;
        job.Result = GenerateFakeResult(jobType, inputPayload);

        dbContext.JobLogs.Add(new JobLog
        {
            JobId = jobId,
            Message = $"{workerName} completed job successfully.",
            Level = "Info",
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task AddJobLogAsync(
        DistributedJobDbContext dbContext,
        Guid jobId,
        string message,
        string level,
        CancellationToken cancellationToken)
    {
        dbContext.JobLogs.Add(new JobLog
        {
            JobId = jobId,
            Message = message,
            Level = level,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateFakeResult(JobType jobType, string inputPayload)
    {
        return jobType switch
        {
            JobType.CsvSummary =>
                $"CSV summary completed for input: {inputPayload}",

            JobType.ReportGeneration =>
                $"Report generation completed for input: {inputPayload}",

            JobType.EmailNotification =>
                $"Email notification simulated for input: {inputPayload}",

            JobType.SecurityScan =>
                $"Security scan simulation completed for input: {inputPayload}",

            _ =>
                $"Job completed for input: {inputPayload}"
        };
    }
}