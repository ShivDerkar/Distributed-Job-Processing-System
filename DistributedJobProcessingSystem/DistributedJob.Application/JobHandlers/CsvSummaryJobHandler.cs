using DistributedJob.Domain.Entities;
using DistributedJob.Domain.Enums;

namespace DistributedJob.Application.JobHandlers;

public class CsvSummaryJobHandler : IJobHandler
{
    public JobType Type => JobType.CsvSummary;

    public async Task<JobProcessingResult> HandleAsync(
        BackgroundJob job,
        CancellationToken cancellationToken)
    {
        await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

        var result =
            $"CSV summary completed. Input received: {job.InputPayload}. " +
            "Rows processed: 1200. Columns detected: 8. Missing values found: 14.";

        return JobProcessingResult.Success(
            result,
            "CSV file validation completed.",
            "CSV summary statistics generated.");
    }
}