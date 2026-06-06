using DistributedJob.Api.Extensions;
using DistributedJob.Application.DTOs;
using DistributedJob.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistributedJob.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;

    public JobsController(IJobService jobService)
    {
        _jobService = jobService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateJob(
        CreateJobRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.GetUserId();

            var response = await _jobService.CreateJobAsync(
                userId,
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetJobById),
                new { jobId = response.Id },
                response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyJobs(
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        var response = await _jobService.GetMyJobsAsync(
            userId,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{jobId:guid}")]
    public async Task<IActionResult> GetJobById(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.GetUserId();

            var response = await _jobService.GetJobByIdAsync(
                userId,
                jobId,
                cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            return NotFound(new
            {
                message = exception.Message
            });
        }
    }

    [HttpPost("{jobId:guid}/cancel")]
    public async Task<IActionResult> CancelJob(
        Guid jobId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = User.GetUserId();

            var response = await _jobService.CancelJobAsync(
                userId,
                jobId,
                cancellationToken);

            return Ok(response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }
}   