using DistributedJob.Domain.Entities;
using DistributedJob.Domain.Enums;

namespace DistributedJob.Application.JobHandlers;

public interface IJobHandler
{
    JobType Type { get; }

    Task<JobProcessingResult> HandleAsync(
        BackgroundJob job,
        CancellationToken cancellationToken);
}