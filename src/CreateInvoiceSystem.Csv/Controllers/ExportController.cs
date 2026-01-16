using CreateInvoiceSystem.Csv.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CreateInvoiceSystem.Csv.Controllers;

[ApiController]
[Route("api/export")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly ICsvExportService _csvService;
    private readonly IExportDataProvider _dataProvider;

    public ExportController(ICsvExportService csvService, IExportDataProvider dataProvider)
    {
        _csvService = csvService;
        _dataProvider = dataProvider;
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> DownloadInvoicesCsv()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId))
            return Unauthorized();

        var data = await _dataProvider.GetInvoicesDataAsync(userId);
        var fileBytes = _csvService.ExportToCsv(data);
        return File(fileBytes, "text/csv", "faktury.csv");
    }
    
    [HttpGet("products")]
    public async Task<IActionResult> DownloadProductsCsv()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId))
            return Unauthorized();

        var data = await _dataProvider.GetProductsDataAsync(userId);
        var fileBytes = _csvService.ExportToCsv(data);
        return File(fileBytes, "text/csv", "produkty.csv");
    }
    
    [HttpGet("clients")]
    public async Task<IActionResult> DownloadClientsCsv()
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId))
            return Unauthorized();

        var data = await _dataProvider.GetClientsDataAsync(userId);
        var fileBytes = _csvService.ExportToCsv(data);
        return File(fileBytes, "text/csv", "klienci.csv");
    }
}
