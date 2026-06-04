using DistributedJob.Domain.Enums;

namespace DistributedJob.Domain.Entities;

public class BackgroundJob
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public JobType Type { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Pending;

    public string InputPayload { get; set; } = string.Empty;

    public string? Result { get; set; }

    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; } = 0;

    public int MaxRetries { get; set; } = 3;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public AppUser? User { get; set; }

    public ICollection<JobLog> Logs { get; set; } = new List<JobLog>();
}