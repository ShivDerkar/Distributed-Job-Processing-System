namespace DistributedJob.Application.JobHandlers;

public class JobProcessingResult
{
    public bool IsSuccess { get; set; }

    public string? Result { get; set; }

    public string? ErrorMessage { get; set; }

    public List<string> Logs { get; set; } = new();

    public static JobProcessingResult Success(
        string result,
        params string[] logs)
    {
        return new JobProcessingResult
        {
            IsSuccess = true,
            Result = result,
            Logs = logs.ToList()
        };
    }

    public static JobProcessingResult Failure(
        string errorMessage,
        params string[] logs)
    {
        return new JobProcessingResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            Logs = logs.ToList()
        };
    }
}