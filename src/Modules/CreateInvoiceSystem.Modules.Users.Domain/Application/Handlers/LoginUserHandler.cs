using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.LoginUser;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;

public class LoginUserHandler(ICommandExecutor commandExecutor, IUserRepository _userRepository, IUserTokenService _tokenService)
    : IRequestHandler<LoginUserRequest, LoginUserResponse>
{
    public async Task<LoginUserResponse> Handle(LoginUserRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginUserCommand(request.Dto, _tokenService);

        var result = await commandExecutor.Execute(
            command,
            _userRepository,
            cancellationToken);
        
        return new LoginUserResponse(
            result.AccessToken,
            !string.IsNullOrEmpty(result.AccessToken),
            result.RefreshToken,
            "Login successful");
    }
}
