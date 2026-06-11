using DistributedJob.Application.Interfaces;
using DistributedJob.Domain.Entities;
using DistributedJob.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DistributedJob.Infrastructure.Repositories;

public class WorkerNodeRepository : IWorkerNodeRepository
{
    private readonly DistributedJobDbContext _dbContext;

    public WorkerNodeRepository(DistributedJobDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<WorkerNode?> GetByNameAsync(
        string workerName,
        CancellationToken cancellationToken)
    {
        return await _dbContext.WorkerNodes
            .FirstOrDefaultAsync(
                worker => worker.WorkerName == workerName,
                cancellationToken);
    }

    public async Task<List<WorkerNode>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        return await _dbContext.WorkerNodes
            .OrderBy(worker => worker.WorkerName)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        WorkerNode workerNode,
        CancellationToken cancellationToken)
    {
        await _dbContext.WorkerNodes.AddAsync(
            workerNode,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}