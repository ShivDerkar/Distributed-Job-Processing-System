using DistributedJob.Application.DTOs;

namespace DistributedJob.Application.Interfaces;

public interface IWorkerMonitoringService
{
    Task<List<WorkerNodeResponse>> GetWorkersAsync(
        CancellationToken cancellationToken);
}