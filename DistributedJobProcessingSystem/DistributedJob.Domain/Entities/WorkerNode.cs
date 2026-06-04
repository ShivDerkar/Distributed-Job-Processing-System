using DistributedJob.Domain.Enums;

namespace DistributedJob.Domain.Entities;

public class WorkerNode
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string WorkerName { get; set; } = string.Empty;

    public WorkerStatus Status { get; set; } = WorkerStatus.Offline;

    public Guid? CurrentJobId { get; set; }

    public DateTime LastHeartbeatAt { get; set; } = DateTime.UtcNow;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}