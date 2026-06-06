using DistributedJob.Domain.Entities;

namespace DistributedJob.Application.Interfaces;

public interface IJobRepository
{
    Task AddAsync(BackgroundJob job, CancellationToken cancellationToken);

    Task<BackgroundJob?> GetByIdForUserAsync(
        Guid jobId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<BackgroundJob?> GetByIdForUserForUpdateAsync(
        Guid jobId,
        Guid userId,
        CancellationToken cancellationToken);

    Task<List<BackgroundJob>> GetJobsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task AddLogAsync(JobLog log, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}