using DistributedJob.Application.JobHandlers;
using DistributedJob.Domain.Enums;

namespace DistributedJob.Application.Factories;

public interface IJobHandlerFactory
{
    IJobHandler GetHandler(JobType jobType);
}