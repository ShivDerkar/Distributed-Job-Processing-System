using DistributedJob.Domain.Entities;
using DistributedJob.Domain.Enums;

namespace DistributedJob.Application.JobHandlers;

public class ReportGenerationJobHandler : IJobHandler
{
    public JobType Type => JobType.ReportGeneration;

    public async Task<JobProcessingResult> HandleAsync(
        BackgroundJob job,
        CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(4), cancellationToken);

        var result =
            $"Report generated successfully for input: {job.InputPayload}. " +
            "Report sections created: Summary, Trends, Risks, Recommendations.";

        return JobProcessingResult.Success(
            result,
            "Report template loaded.",
            "Report sections generated.",
            "Report generation completed.");
    }
}