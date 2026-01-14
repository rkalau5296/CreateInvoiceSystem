using CreateInvoiceSystem.Abstractions.ControllerBase;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ForgotPassword;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.LoginUser;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RegisterUser;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace CreateInvoiceSystem.Modules.Users.Domain.Controllers;

[Route("api/[controller]")]
public class AuthController : ApiControllerBase
{
    public AuthController(IMediator mediator, ILogger<AuthController> logger) : base(mediator)
    {
        logger.LogInformation("This is AuthController");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        return await HandleRequest<RegisterUserRequest, RegisterUserResponse>(request, cancellationToken);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LoginAsync([FromBody] LoginUserRequest request, CancellationToken cancellationToken)
    {   
        return await HandleRequest<LoginUserRequest, LoginUserResponse>(request, cancellationToken);
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        return await HandleRequest<ForgotPasswordRequest, ForgotPasswordResponse>(request, cancellationToken);
    }
}
