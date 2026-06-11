using DistributedJob.Application.DTOs;
using DistributedJob.Application.Interfaces;

namespace DistributedJob.Application.Services;

public class WorkerMonitoringService : IWorkerMonitoringService
{
    private readonly IWorkerNodeRepository _workerNodeRepository;

    public WorkerMonitoringService(IWorkerNodeRepository workerNodeRepository)
    {
        _workerNodeRepository = workerNodeRepository;
    }

    public async Task<List<WorkerNodeResponse>> GetWorkersAsync(
        CancellationToken cancellationToken)
    {
        var workers = await _workerNodeRepository.GetAllAsync(cancellationToken);

        return workers
            .Select(worker =>
            {
                var secondsSinceHeartbeat =
                    (DateTime.UtcNow - worker.LastHeartbeatAt).TotalSeconds;

                return new WorkerNodeResponse
                {
                    Id = worker.Id,
                    WorkerName = worker.WorkerName,
                    Status = worker.Status,
                    CurrentJobId = worker.CurrentJobId,
                    LastHeartbeatAt = worker.LastHeartbeatAt,
                    CreatedAt = worker.CreatedAt,
                    SecondsSinceLastHeartbeat = Math.Round(secondsSinceHeartbeat, 2),
                    IsHealthy = secondsSinceHeartbeat <= 30
                };
            })
            .ToList();
    }
}