using DistributedJob.Application.JobHandlers;
using DistributedJob.Domain.Enums;

namespace DistributedJob.Application.Factories;

public class JobHandlerFactory : IJobHandlerFactory
{
    private readonly Dictionary<JobType, IJobHandler> _handlers;

    public JobHandlerFactory(IEnumerable<IJobHandler> handlers)
    {
        _handlers = handlers.ToDictionary(
            handler => handler.Type,
            handler => handler);
    }

    public IJobHandler GetHandler(JobType jobType)
    {
        if (_handlers.TryGetValue(jobType, out var handler))
        {
            return handler;
        }

        throw new InvalidOperationException(
            $"No job handler registered for job type {jobType}.");
    }
}