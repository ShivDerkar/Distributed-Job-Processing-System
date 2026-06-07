namespace DistributedJob.Application.Interfaces;

public interface IJobQueue
{
    Task EnqueueAsync(Guid jobId, CancellationToken cancellationToken);

    Task<Guid?> DequeueAsync(CancellationToken cancellationToken);
}