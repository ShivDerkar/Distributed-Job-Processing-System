namespace DistributedJob.Domain.Entities;

public class JobLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid JobId { get; set; }

    public string Message { get; set; } = string.Empty;

    public string Level { get; set; } = "Info";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public BackgroundJob? Job { get; set; }
}