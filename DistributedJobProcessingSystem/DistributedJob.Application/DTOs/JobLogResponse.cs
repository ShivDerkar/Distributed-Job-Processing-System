namespace DistributedJob.Application.DTOs;

public class JobLogResponse
{
    public Guid Id { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Level { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}