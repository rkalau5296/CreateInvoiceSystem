namespace CreateInvoiceSystem.API.Controllers;

using CreateInvoiceSystem.Addresses.Application.DTO;
using CreateInvoiceSystem.Addresses.Application.RequestsResponses.CreateAddress;
using CreateInvoiceSystem.Addresses.Application.RequestsResponses.DeleteAddress;
using CreateInvoiceSystem.Addresses.Application.RequestsResponses.GetAddress;
using CreateInvoiceSystem.Addresses.Application.RequestsResponses.GetAddresses;
using CreateInvoiceSystem.Addresses.Application.RequestsResponses.UpdateAddress;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class AddressController(IMediator _mediator) : Controller
{
    [HttpGet("{addressId}")]
    [ProducesResponseType(typeof(GetAddressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAddressAsync([FromRoute] int addressId, CancellationToken cancellationToken) 
    {
        GetAddressRequest request = new(addressId);
        GetAddressResponse response = await _mediator.Send(request, cancellationToken);

        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpGet()]
    [Route("/Addresses")]
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

    [HttpGet("throw")]
    public IActionResult Throw()
    {
        throw new FluentValidation.ValidationException("test", new List<FluentValidation.Results.ValidationFailure>());
    }
}
