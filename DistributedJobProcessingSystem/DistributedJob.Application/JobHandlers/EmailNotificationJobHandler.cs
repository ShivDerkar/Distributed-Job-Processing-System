using DistributedJob.Domain.Entities;
using DistributedJob.Domain.Enums;

namespace DistributedJob.Application.JobHandlers;

public class EmailNotificationJobHandler : IJobHandler
{
    public JobType Type => JobType.EmailNotification;

    public async Task<JobProcessingResult> HandleAsync(
        BackgroundJob job,
        CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);

        var result =
            $"Email notification simulated successfully. Input: {job.InputPayload}.";

        return JobProcessingResult.Success(
            result,
            "Email payload validated.",
            "Email notification simulation completed.");
    }
}