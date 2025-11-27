namespace CreateInvoiceSystem.API.Controllers;

using CreateInvoiceSystem.Abstractions.ControllerBase;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Users.Application.RequestsResponses.CreateUser;
using CreateInvoiceSystem.Users.Application.RequestsResponses.DeleteUser;
using CreateInvoiceSystem.Users.Application.RequestsResponses.GetUser;
using CreateInvoiceSystem.Users.Application.RequestsResponses.GetUsers;
using CreateInvoiceSystem.Users.Application.RequestsResponses.UpdateUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class UserController : ApiControllerBase
{
    public UserController(IMediator mediator, ILogger<UserController> logger) : base(mediator)
    {
        logger.LogInformation("This is AddressController");
    }

    [HttpGet("{UserId}")]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetUserAsync([FromRoute] int UserId, CancellationToken cancellationToken)
    {
        GetUserRequest request = new(UserId);
        return this.HandleRequest<GetUserRequest, GetUserResponse>(request, cancellationToken);
    }

    [HttpGet()]
    [Route("/Users")]
    public async Task<IActionResult> GetUsersAsync([FromQuery] GetUsersRequest request, CancellationToken cancellationToken)
    {
        return await this.HandleRequest<GetUsersRequest, GetUsersResponse>(request, cancellationToken);
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateUsersAsync([FromBody] UserDto UserDto, CancellationToken cancellationToken)
    {
        CreateUserRequest request = new(UserDto);
        return await this.HandleRequest<CreateUserRequest, CreateUserResponse>(request, cancellationToken);
    }

    [HttpPut]
    [Route("update/id")]
    public async Task<IActionResult> UpdateUserAsync(int id, [FromBody] UserDto UserDto, CancellationToken cancellationToken)
    {
        UpdateUserRequest request = new(id, UserDto);
        return await this.HandleRequest<UpdateUserRequest, UpdateUserResponse>(request, cancellationToken);
    }

    [HttpDelete]
    [Route("id")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
    {
        DeleteUserRequest request = new(id);
        return await this.HandleRequest<DeleteUserRequest, DeleteUserResponse>(request, cancellationToken);
    }

}