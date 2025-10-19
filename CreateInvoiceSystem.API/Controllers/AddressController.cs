using CreateInvoiceSystem.Address.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;

namespace CreateInvoiceSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : Controller
    {
        private readonly IMediator _mediator;        

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddressAsync([FromRoute] int id, CancellationToken cancellationToken) 
        {
            GetAddressRequest request = new(id);
            GetAddressResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }       

        [HttpGet]
        public async Task<IActionResult> GetTrucksAsync([FromQuery] GetTrucksRequest request, CancellationToken cancellationToken) =>
            (await _mediator.Send(_mapper.MapToMessage(request), cancellationToken)).Match(Ok, this.ErrorResult);

        [HttpPost]
        public async Task<IActionResult> CreateTruckAsync([FromBody] CreateTruckRequest request, CancellationToken cancellationToken) =>
            (await _mediator.Send(_mapper.MapToMessage(request), cancellationToken)).Match(Ok, this.ErrorResult);

        [HttpPut]
        public async Task<IActionResult> UpdateTruckAsync([FromBody] UpdateTruckRequest request, CancellationToken cancellationToken) =>
            (await _mediator.Send(_mapper.MapToMessage(request), cancellationToken)).Match(Ok, this.ErrorResult);

        [HttpDelete("{idOrCode}")]
        public async Task<IActionResult> DeleteTruckAsync([FromRoute] string idOrCode, CancellationToken cancellationToken) =>
            (await _mediator.Send(new DeleteTruckCommand() { IdOrCode = idOrCode }, cancellationToken)).Match(Ok, this.ErrorResult);
    }
}
