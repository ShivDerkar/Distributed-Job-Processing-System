using DistributedJob.Domain.Enums;

namespace DistributedJob.Application.DTOs;

public class CreateJobRequest
{
    public JobType Type { get; set; }

    public string InputPayload { get; set; } = string.Empty;
}