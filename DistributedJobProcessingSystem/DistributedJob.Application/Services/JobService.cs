using DistributedJob.Application.DTOs;
using DistributedJob.Application.Interfaces;
using DistributedJob.Domain.Entities;
using DistributedJob.Domain.Enums;

namespace DistributedJob.Application.Services;

public class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
private readonly IJobQueue _jobQueue;

public JobService(
    IJobRepository jobRepository,
    IJobQueue jobQueue)
{
    _jobRepository = jobRepository;
    _jobQueue = jobQueue;
}


     public async Task<JobResponse> CreateJobAsync(
    Guid userId,
    CreateJobRequest request,
    CancellationToken cancellationToken)
{
    if (string.IsNullOrWhiteSpace(request.InputPayload))
    {
        throw new InvalidOperationException("Input payload is required.");
    }

    var job = new BackgroundJob
    {
        UserId = userId,
        Type = request.Type,
        Status = JobStatus.Pending,
        InputPayload = request.InputPayload.Trim(),
        RetryCount = 0,
        MaxRetries = 3,
        CreatedAt = DateTime.UtcNow
    };

    job.Logs.Add(new JobLog
    {
        JobId = job.Id,
        Message = "Job created and stored in PostgreSQL.",
        Level = "Info",
        CreatedAt = DateTime.UtcNow
    });

    job.Logs.Add(new JobLog
    {
        JobId = job.Id,
        Message = "Job is ready to be enqueued in Redis.",
        Level = "Info",
        CreatedAt = DateTime.UtcNow
    });

    // Save all database changes first
    await _jobRepository.AddAsync(job, cancellationToken);
    await _jobRepository.SaveChangesAsync(cancellationToken);

    // Only after DB save is complete, push job ID to Redis
    await _jobQueue.EnqueueAsync(job.Id, cancellationToken);

    return MapToResponse(job);
}   

    public async Task<List<JobResponse>> GetMyJobsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var jobs = await _jobRepository.GetJobsForUserAsync(
            userId,
            cancellationToken);

        return jobs.Select(MapToResponse).ToList();
    }

    public async Task<JobResponse> GetJobByIdAsync(
        Guid userId,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdForUserAsync(
            jobId,
            userId,
            cancellationToken);

        if (job is null)
        {
            throw new InvalidOperationException("Job not found.");
        }

        return MapToResponse(job);
    }

    public async Task<JobResponse> CancelJobAsync(
        Guid userId,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdForUserAsync(
            jobId,
            userId,
            cancellationToken);

        if (job is null)
        {
            throw new InvalidOperationException("Job not found.");
        }

        if (job.Status is JobStatus.Succeeded or JobStatus.Failed or JobStatus.Cancelled)
        {
            throw new InvalidOperationException(
                $"Job cannot be cancelled because it is already {job.Status}.");
        }

        job.Status = JobStatus.Cancelled;
        job.CompletedAt = DateTime.UtcNow;

        job.Logs.Add(new JobLog
        {
            JobId = job.Id,
            Message = "Job was cancelled by the user.",
            Level = "Warning",
            CreatedAt = DateTime.UtcNow
        });

        await _jobRepository.SaveChangesAsync(cancellationToken);

        return MapToResponse(job);
    }

    private static JobResponse MapToResponse(BackgroundJob job)
    {
        return new JobResponse
        {
            Id = job.Id,
            Type = job.Type,
            Status = job.Status,
            InputPayload = job.InputPayload,
            Result = job.Result,
            ErrorMessage = job.ErrorMessage,
            RetryCount = job.RetryCount,
            MaxRetries = job.MaxRetries,
            CreatedAt = job.CreatedAt,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            Logs = job.Logs
                .OrderBy(log => log.CreatedAt)
                .Select(log => new JobLogResponse
                {
                    Id = log.Id,
                    Message = log.Message,
                    Level = log.Level,
                    CreatedAt = log.CreatedAt
                })
                .ToList()
        };
    }
}