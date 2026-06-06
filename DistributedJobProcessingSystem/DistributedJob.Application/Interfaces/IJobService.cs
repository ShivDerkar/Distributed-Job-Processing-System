using DistributedJob.Application.DTOs;

namespace DistributedJob.Application.Interfaces;

public interface IJobService
{
    Task<JobResponse> CreateJobAsync(
        Guid userId,
        CreateJobRequest request,
        CancellationToken cancellationToken);

    Task<List<JobResponse>> GetMyJobsAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<JobResponse> GetJobByIdAsync(
        Guid userId,
        Guid jobId,
        CancellationToken cancellationToken);

    Task<JobResponse> CancelJobAsync(
        Guid userId,
        Guid jobId,
        CancellationToken cancellationToken);
}