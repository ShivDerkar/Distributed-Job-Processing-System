using DistributedJob.Application.Factories;
using DistributedJob.Application.Interfaces;
using DistributedJob.Application.JobHandlers;
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

        var jobTimeoutSeconds = int.Parse(
            _configuration["Worker:JobTimeoutSeconds"] ?? "10");

        var retryDelaySeconds = int.Parse(
            _configuration["Worker:RetryDelaySeconds"] ?? "3");

        var heartbeatIntervalSeconds = int.Parse(
            _configuration["Worker:HeartbeatIntervalSeconds"] ?? "5");

        _logger.LogInformation("{WorkerName} started.", workerName);


        using (var startupScope = _scopeFactory.CreateScope())
{
    var startupDbContext =
        startupScope.ServiceProvider.GetRequiredService<DistributedJobDbContext>();

    await RegisterOrUpdateWorkerAsync(
        startupDbContext,
        workerName,
        WorkerStatus.Online,
        null,
        stoppingToken);
}
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var jobQueue = scope.ServiceProvider.GetRequiredService<IJobQueue>();
                var dbContext = scope.ServiceProvider.GetRequiredService<DistributedJobDbContext>();
                var jobHandlerFactory = scope.ServiceProvider.GetRequiredService<IJobHandlerFactory>();

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
                await RegisterOrUpdateWorkerAsync(
    dbContext,
    workerName,
    WorkerStatus.Busy,
    jobId,
    stoppingToken);

                var job = await dbContext.Jobs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == jobId.Value, stoppingToken);

                if (jobId is null)
{
    await RegisterOrUpdateWorkerAsync(
        dbContext,
        workerName,
        WorkerStatus.Online,
        null,
        stoppingToken);

    _logger.LogInformation("{WorkerName} found no pending jobs.", workerName);

    await Task.Delay(
        TimeSpan.FromSeconds(pollingDelaySeconds),
        stoppingToken);

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

                if (job.Status == JobStatus.Succeeded || job.Status == JobStatus.Failed)
                {
                    await AddJobLogAsync(
                        dbContext,
                        job.Id,
                        $"{workerName} skipped job because it was already {job.Status}.",
                        "Warning",
                        stoppingToken);

                    continue;
                }

                await MarkJobRunningAsync(
                    dbContext,
                    job.Id,
                    workerName,
                    stoppingToken);

                var handler = jobHandlerFactory.GetHandler(job.Type);

                var processingResult = await ExecuteHandlerWithTimeoutAsync(
                    handler,
                    job,
                    jobTimeoutSeconds,
                    stoppingToken);

                if (processingResult.IsSuccess)
                {
                    await MarkJobSucceededAsync(
                        dbContext,
                        job.Id,
                        processingResult.Result ?? "Job completed successfully.",
                        processingResult.Logs,
                        workerName,
                        stoppingToken);
                }
                else
                {
                    await HandleJobFailureAsync(
                        dbContext,
                        jobQueue,
                        job.Id,
                        processingResult.ErrorMessage ?? "Job failed.",
                        processingResult.Logs,
                        workerName,
                        retryDelaySeconds,
                        stoppingToken);
                }
                await RegisterOrUpdateWorkerAsync(
    dbContext,
    workerName,
    WorkerStatus.Online,
    null,
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
        using (var shutdownScope = _scopeFactory.CreateScope())
{
    var shutdownDbContext =
        shutdownScope.ServiceProvider.GetRequiredService<DistributedJobDbContext>();

    await RegisterOrUpdateWorkerAsync(
        shutdownDbContext,
        workerName,
        WorkerStatus.Offline,
        null,
        CancellationToken.None);
}
        _logger.LogInformation("{WorkerName} stopped.", workerName);
    }

    private static async Task<JobProcessingResult> ExecuteHandlerWithTimeoutAsync(
        IJobHandler handler,
        BackgroundJob job,
        int timeoutSeconds,
        CancellationToken stoppingToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);

        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        try
        {
            return await handler.HandleAsync(job, timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
        {
            return JobProcessingResult.Failure(
                $"Job timed out after {timeoutSeconds} seconds.",
                "Job processing timeout reached.");
        }
        catch (Exception exception)
        {
            return JobProcessingResult.Failure(
                exception.Message,
                "Handler threw an unexpected exception.");
        }
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
        job.CompletedAt = null;

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
        string result,
        List<string> handlerLogs,
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
        job.Result = result;
        job.ErrorMessage = null;

        foreach (var log in handlerLogs)
        {
            dbContext.JobLogs.Add(new JobLog
            {
                JobId = jobId,
                Message = log,
                Level = "Info",
                CreatedAt = DateTime.UtcNow
            });
        }

        dbContext.JobLogs.Add(new JobLog
        {
            JobId = jobId,
            Message = $"{workerName} completed job successfully.",
            Level = "Info",
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task HandleJobFailureAsync(
        DistributedJobDbContext dbContext,
        IJobQueue jobQueue,
        Guid jobId,
        string errorMessage,
        List<string> handlerLogs,
        string workerName,
        int retryDelaySeconds,
        CancellationToken cancellationToken)
    {
        var job = await dbContext.Jobs
            .FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken);

        if (job is null)
        {
            return;
        }

        foreach (var log in handlerLogs)
        {
            dbContext.JobLogs.Add(new JobLog
            {
                JobId = jobId,
                Message = log,
                Level = "Warning",
                CreatedAt = DateTime.UtcNow
            });
        }

        if (job.RetryCount < job.MaxRetries)
        {
            job.RetryCount++;
            job.Status = JobStatus.Pending;
            job.ErrorMessage = errorMessage;
            job.CompletedAt = null;

            dbContext.JobLogs.Add(new JobLog
            {
                JobId = jobId,
                Message = $"{workerName} failed job attempt. Reason: {errorMessage}",
                Level = "Error",
                CreatedAt = DateTime.UtcNow
            });

            dbContext.JobLogs.Add(new JobLog
            {
                JobId = jobId,
                Message = $"Retry {job.RetryCount} of {job.MaxRetries} scheduled.",
                Level = "Warning",
                CreatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync(cancellationToken);

            await Task.Delay(
                TimeSpan.FromSeconds(retryDelaySeconds),
                cancellationToken);

            await jobQueue.EnqueueAsync(jobId, cancellationToken);

            return;
        }

        job.Status = JobStatus.Failed;
        job.CompletedAt = DateTime.UtcNow;
        job.ErrorMessage = errorMessage;

        dbContext.JobLogs.Add(new JobLog
        {
            JobId = jobId,
            Message = $"{workerName} marked job as Failed after reaching max retries. Reason: {errorMessage}",
            Level = "Error",
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
    private static async Task RegisterOrUpdateWorkerAsync(
    DistributedJobDbContext dbContext,
    string workerName,
    WorkerStatus status,
    Guid? currentJobId,
    CancellationToken cancellationToken)
{
    var worker = await dbContext.WorkerNodes
        .FirstOrDefaultAsync(x => x.WorkerName == workerName, cancellationToken);

    if (worker is null)
    {
        worker = new WorkerNode
        {
            WorkerName = workerName,
            Status = status,
            CurrentJobId = currentJobId,
            LastHeartbeatAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.WorkerNodes.Add(worker);
    }
    else
    {
        worker.Status = status;
        worker.CurrentJobId = currentJobId;
        worker.LastHeartbeatAt = DateTime.UtcNow;
    }

    await dbContext.SaveChangesAsync(cancellationToken);
}
}