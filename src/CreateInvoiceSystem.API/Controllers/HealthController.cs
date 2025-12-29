using Microsoft.AspNetCore.Mvc;

namespace CreateInvoiceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet("ping")]
    public IActionResult Ping() => Ok(new { status = "ok", timestamp = DateTimeOffset.UtcNow });
}