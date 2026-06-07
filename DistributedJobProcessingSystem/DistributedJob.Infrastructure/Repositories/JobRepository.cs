using DistributedJob.Application.Interfaces;
using DistributedJob.Domain.Entities;
using DistributedJob.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DistributedJob.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly DistributedJobDbContext _dbContext;

    public JobRepository(DistributedJobDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        BackgroundJob job,
        CancellationToken cancellationToken)
    {
        await _dbContext.Jobs.AddAsync(job, cancellationToken);
    }

    public async Task<BackgroundJob?> GetByIdAsync(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Jobs
            .Include(job => job.Logs)
            .FirstOrDefaultAsync(
                job => job.Id == jobId,
                cancellationToken);
    }

    public async Task<BackgroundJob?> GetByIdForUserAsync(
        Guid jobId,
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Jobs
            .Include(job => job.Logs)
            .FirstOrDefaultAsync(
                job => job.Id == jobId && job.UserId == userId,
                cancellationToken);
    }

    public async Task<List<BackgroundJob>> GetJobsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Jobs
            .Include(job => job.Logs)
            .Where(job => job.UserId == userId)
            .OrderByDescending(job => job.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}