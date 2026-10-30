using System.Security.Claims;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreateInvoiceSystem.Modules.Ksef.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class KsefController(IKsefOutboxRepository outboxRepository) : ControllerBase
{
    [HttpPost("{invoiceId:int}/send")]
    public async Task<IActionResult> SendToKsefAsync([FromRoute] int invoiceId, CancellationToken cancellationToken)
    {
        var claimValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();

        var outboxId = await outboxRepository.EnqueueAsync(actualUserId, invoiceId, cancellationToken);        
        return Accepted(new { OutboxId = outboxId });
    }
}