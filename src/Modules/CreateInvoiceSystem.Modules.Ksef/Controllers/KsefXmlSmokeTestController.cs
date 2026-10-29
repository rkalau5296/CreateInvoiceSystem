using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CreateInvoiceSystem.Modules.Ksef.Controllers;

[Route("ksef-xml-smoke-test")]
[ApiController]
public class KsefXmlSmokeTestController(IKsefInvoiceXmlGenerator xmlGenerator) : ControllerBase
{
    [HttpGet("{userId:int}/{invoiceId:int}")]
    public async Task<IActionResult> GenerateXml(int userId, int invoiceId, CancellationToken cancellationToken)
    {
        var xml = await xmlGenerator.GenerateXmlAsync(userId, invoiceId, cancellationToken);
        return Content(xml, "application/xml");
    }
}