using Microsoft.AspNetCore.Mvc;

namespace DistributedJob.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "Distributed Job Processing API",
            timestamp = DateTime.UtcNow
        });
    }
}