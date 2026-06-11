using DistributedJob.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistributedJob.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkersController : ControllerBase
{
    private readonly IWorkerMonitoringService _workerMonitoringService;

    public WorkersController(IWorkerMonitoringService workerMonitoringService)
    {
        _workerMonitoringService = workerMonitoringService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWorkers(
        CancellationToken cancellationToken)
    {
        var workers = await _workerMonitoringService.GetWorkersAsync(
            cancellationToken);

        return Ok(workers);
    }
}