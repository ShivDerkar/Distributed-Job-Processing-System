using DistributedJob.Domain.Enums;

namespace DistributedJob.Application.DTOs;

public class WorkerNodeResponse
{
    public Guid Id { get; set; }

    public string WorkerName { get; set; } = string.Empty;

    public WorkerStatus Status { get; set; }

    public Guid? CurrentJobId { get; set; }

    public DateTime LastHeartbeatAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public double SecondsSinceLastHeartbeat { get; set; }

    public bool IsHealthy { get; set; }
}