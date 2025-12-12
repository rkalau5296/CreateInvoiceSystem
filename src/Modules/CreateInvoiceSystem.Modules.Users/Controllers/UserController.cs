using CreateInvoiceSystem.Abstractions.ControllerBase;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.CreateUser;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.DeleteUser;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.GetUser;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.GetUsers;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.UpdateUser;
using CreateInvoiceSystem.Modules.Users.Dto;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CreateInvoiceSystem.Modules.Users.Controllers;


[ApiController]
[Route("[controller]")]
public class UserController : ApiControllerBase
{
    public UserController(IMediator mediator, ILogger<UserController> logger) : base(mediator)
    {
        logger.LogInformation("This is UserController");
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(GetUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetUserAsync([FromRoute] int userId, CancellationToken cancellationToken)
    {
        GetUserRequest request = new(userId);
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
    public async Task<IActionResult> CreateUsersAsync([FromBody] CreateUserDto userDto, CancellationToken cancellationToken)
    {
        CreateUserRequest request = new(userDto);
        return await this.HandleRequest<CreateUserRequest, CreateUserResponse>(request, cancellationToken);
    }

    [HttpPut]
    [Route("update/id")]
    public async Task<IActionResult> UpdateUserAsync(int id, [FromBody] UpdateUserDto userDto, CancellationToken cancellationToken)
    {
        UpdateUserRequest request = new(userDto, id);
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