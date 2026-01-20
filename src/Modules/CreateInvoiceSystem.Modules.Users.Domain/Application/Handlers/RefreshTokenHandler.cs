using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RefreshToken;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;

public class RefreshTokenHandler(
    IUserRepository _userRepository,
    IUserAuthService _userAuthService) : IRequestHandler<RefreshTokenRequest, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(request, _userRepository, _userAuthService);

        return await command.ExecuteAsync(cancellationToken);
    }
}
