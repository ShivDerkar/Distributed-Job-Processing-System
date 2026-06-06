using DistributedJob.Domain.Enums;

namespace DistributedJob.Application.DTOs;

public class JobResponse
{
    public Guid Id { get; set; }

    public JobType Type { get; set; }

    public JobStatus Status { get; set; }

    public string InputPayload { get; set; } = string.Empty;

    public string? Result { get; set; }

    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; }

    public int MaxRetries { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public List<JobLogResponse> Logs { get; set; } = new();
}