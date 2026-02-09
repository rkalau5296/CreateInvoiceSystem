using CreateInvoiceSystem.Abstractions.ControllerBase;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.CreateUser;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.DeleteUser;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.GetUser;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.GetUsers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RefreshToken;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.UpdateUser;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CreateInvoiceSystem.Modules.Users.Domain.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
        return HandleRequest<GetUserRequest, GetUserResponse>(request, cancellationToken);
    }

    [HttpGet]    
    public async Task<IActionResult> GetUsersAsync([FromQuery] GetUsersRequest request, CancellationToken cancellationToken)
    {
        return await HandleRequest<GetUsersRequest, GetUsersResponse>(request, cancellationToken);
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateUsersAsync([FromBody] CreateUserDto userDto, CancellationToken cancellationToken)
    {
        CreateUserRequest request = new(userDto);
        return await HandleRequest<CreateUserRequest, CreateUserResponse>(request, cancellationToken);
    }

    [HttpPut]
    [Route("update/{id}")]
    public async Task<IActionResult> UpdateUserAsync(int id, [FromBody] UpdateUserDto userDto, CancellationToken cancellationToken)
    {
        var claimValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();      
        
        var secureDto = userDto with { UserId = actualUserId };

        UpdateUserRequest request = new(secureDto, id);
        return await HandleRequest<UpdateUserRequest, UpdateUserResponse>(request, cancellationToken);
    }

    [HttpDelete]
    [Route("{id}")]    
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
    {
        DeleteUserRequest request = new(id);
        return await HandleRequest<DeleteUserRequest, DeleteUserResponse>(request, cancellationToken);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] Guid refreshToken, CancellationToken cancellationToken)
    {        
        RefreshTokenRequest request = new(refreshToken);

        return await HandleRequest<RefreshTokenRequest, AuthResponse>(request, cancellationToken);
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyDataAsync(CancellationToken cancellationToken)
    {
        var claimValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();
        
        GetUserRequest request = new(actualUserId);
        return await HandleRequest<GetUserRequest, GetUserResponse>(request, cancellationToken);
    }
}