namespace CreateInvoiceSystem.API.Controllers;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using CreateInvoiceSystem.Address.Application.DTO;
using CreateInvoiceSystem.Address.Application.RequestsResponses.GetAddress;
using CreateInvoiceSystem.Address.Application.RequestsResponses.GetAddresses;
using CreateInvoiceSystem.Address.Application.RequestsResponses.CreateAddress;

[ApiController]
[Route("api/[controller]")]
public class AddressController(IMediator _mediator) : Controller
{        

    [HttpGet("{addressId}")]
    public async Task<IActionResult> GetAddressAsync([FromRoute] int addressId, CancellationToken cancellationToken) 
    {
        GetAddressRequest request = new(addressId);
        GetAddressResponse response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet()]
    public async Task<IActionResult> GetAddressesAsync([FromQuery] GetAddressesRequest request, CancellationToken cancellationToken)
    {
        GetAddressesResponse response = await _mediator.Send(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateAddressAsync([FromBody] AddressDto addressDto)
    {
        CreateAddressRequest request = new(addressDto);
        CreateAddressResponse response = await _mediator.Send(request);

        return Ok(response);
    }


}
