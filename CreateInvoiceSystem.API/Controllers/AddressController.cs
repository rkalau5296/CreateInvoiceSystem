namespace CreateInvoiceSystem.API.Controllers;

using CreateInvoiceSystem.Address.Application.DTO;
using CreateInvoiceSystem.Address.Application.RequestsResponses.CreateAddress;
using CreateInvoiceSystem.Address.Application.RequestsResponses.DeleteAddress;
using CreateInvoiceSystem.Address.Application.RequestsResponses.GetAddress;
using CreateInvoiceSystem.Address.Application.RequestsResponses.GetAddresses;
using CreateInvoiceSystem.Address.Application.RequestsResponses.UpdateAddress;
using MediatR;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> CreateAddressAsync([FromBody] AddressDto addressDto, CancellationToken cancellationToken)
    {
        CreateAddressRequest request = new(addressDto);
        CreateAddressResponse response = await _mediator.Send(request, cancellationToken);

        return Ok(response);
    }

    [HttpPut]
    [Route("update/id")]
    public async Task<IActionResult> UpdateAddressAsync(int id, [FromBody] AddressDto addressDto, CancellationToken cancellationToken)
    {
        PutAddressRequest updatedAddress = new(id, addressDto);
        PutAddressResponse response = await _mediator.Send(updatedAddress, cancellationToken);

        return Ok(response);
    }

    [HttpDelete]
    [Route("id")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        DeleteAddressRequest request = new(id);
        DeleteAddressResponse response = await _mediator.Send(request);

        return Ok(response);
    }
}
