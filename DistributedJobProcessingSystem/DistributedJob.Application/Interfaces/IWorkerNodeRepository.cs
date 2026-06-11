using DistributedJob.Domain.Entities;

namespace DistributedJob.Application.Interfaces;

public interface IWorkerNodeRepository
{
    Task<WorkerNode?> GetByNameAsync(
        string workerName,
        CancellationToken cancellationToken);

    Task<List<WorkerNode>> GetAllAsync(
        CancellationToken cancellationToken);

    Task AddAsync(
        WorkerNode workerNode,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(
        CancellationToken cancellationToken);
}