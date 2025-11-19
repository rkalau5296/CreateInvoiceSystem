namespace CreateInvoiceSystem.API.Controllers;

using CreateInvoiceSystem.Abstractions.ControllerBase;
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
public class AddressController(IMediator mediator) : ApiControllerBase(mediator)
{
    [HttpGet("{addressId}")]
    [ProducesResponseType(typeof(GetAddressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetAddressAsync([FromRoute] int addressId, CancellationToken cancellationToken) 
    {
        GetAddressRequest request = new(addressId);
        return this.HandleRequest<GetAddressRequest, GetAddressResponse>(request, cancellationToken);
    }

    [HttpGet()]
    [Route("/Addresses")]
    public async Task<IActionResult> GetAddressesAsync([FromQuery] GetAddressesRequest request, CancellationToken cancellationToken)
    {
        return await this.HandleRequest<GetAddressesRequest, GetAddressesResponse>(request, cancellationToken);
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateAddressAsync([FromBody] AddressDto addressDto, CancellationToken cancellationToken)
    {        
        CreateAddressRequest request = new(addressDto);
        return await this.HandleRequest<CreateAddressRequest, CreateAddressResponse>(request, cancellationToken);
    }

    [HttpPut]
    [Route("update/id")]
    public async Task<IActionResult> UpdateAddressAsync(int id, [FromBody] AddressDto addressDto, CancellationToken cancellationToken)
    {
        UpdateAddressRequest request = new(id, addressDto);
        return await this.HandleRequest<UpdateAddressRequest, UpdateAddressResponse>(request, cancellationToken);
    }

    [HttpDelete]
    [Route("id")]
    public async Task<IActionResult> DeleteAddress(int id, CancellationToken cancellationToken)
    {
        DeleteAddressRequest request = new(id);
        return await this.HandleRequest<DeleteAddressRequest, DeleteAddressResponse>(request, cancellationToken);
    }    
}
