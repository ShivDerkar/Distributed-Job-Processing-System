using DistributedJob.Application.Interfaces;
using StackExchange.Redis;

namespace DistributedJob.Infrastructure.Queue;

public class RedisJobQueue : IJobQueue
{
    private const string PendingJobsQueueName = "jobs:pending";

    private readonly IDatabase _database;

    public RedisJobQueue(IConnectionMultiplexer connectionMultiplexer)
    {
        _database = connectionMultiplexer.GetDatabase();
    }

    public async Task EnqueueAsync(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        await _database.ListLeftPushAsync(
            PendingJobsQueueName,
            jobId.ToString());
    }

    public async Task<Guid?> DequeueAsync(CancellationToken cancellationToken)
    {
        var value = await _database.ListRightPopAsync(PendingJobsQueueName);

        if (value.IsNullOrEmpty)
        {
            return null;
        }

        var jobIdText = value.ToString();

        if (!Guid.TryParse(jobIdText, out var jobId))
        {
            return null;
        }

        return jobId;
    }
}