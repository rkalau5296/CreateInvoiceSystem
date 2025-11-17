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
public class AddressController : Controller
{
    private readonly IMediator _mediator;

    public AddressController(IMediator mediator)
    {
        _mediator = mediator;
    }

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
        UpdateAddressRequest updatedAddress = new(id, addressDto);
        UpdateAddressResponse response = await _mediator.Send(updatedAddress, cancellationToken);

        return Ok(response);
    }

    [HttpDelete]
    [Route("id")]
    public async Task<IActionResult> DeleteAddress(int id, CancellationToken cancellationToken)
    {
        DeleteAddressRequest request = new(id);
        await _mediator.Send(request, cancellationToken);

        return NoContent();
    }
}
