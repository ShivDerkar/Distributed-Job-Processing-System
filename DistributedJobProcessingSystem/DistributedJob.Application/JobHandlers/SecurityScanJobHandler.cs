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