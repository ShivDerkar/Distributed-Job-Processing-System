using DistributedJob.Domain.Entities;
using DistributedJob.Domain.Enums;

namespace DistributedJob.Application.JobHandlers;

public class SecurityScanJobHandler : IJobHandler
{
    public JobType Type => JobType.SecurityScan;

    public async Task<JobProcessingResult> HandleAsync(
        BackgroundJob job,
        CancellationToken cancellationToken)
    {
        if (job.InputPayload.Contains("forceFail", StringComparison.OrdinalIgnoreCase))
        {
            return JobProcessingResult.Failure(
                "Security scan failed because a critical simulated vulnerability was found.",
                "Security scan initialized.",
                "Critical vulnerability simulation triggered.");
        }

        if (job.InputPayload.Contains("forceTimeout", StringComparison.OrdinalIgnoreCase))
        {
            await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
        }

        await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

        var result =
            $"Security scan completed for input: {job.InputPayload}. " +
            "Critical: 0, High: 1, Medium: 3, Low: 5.";

        return JobProcessingResult.Success(
            result,
            "Security scan initialized.",
            "Static analysis simulation completed.",
            "Security scan result generated.");
    }
}