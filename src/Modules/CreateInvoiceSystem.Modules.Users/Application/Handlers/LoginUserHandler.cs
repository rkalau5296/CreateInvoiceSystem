using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.LoginUser;
using CreateInvoiceSystem.Modules.Users.Entities;
using CreateInvoiceSystem.Modules.Users.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Application.Handlers;

public class LoginUserHandler(IUserRepository userRepository, IUserTokenService tokenService)
    : IRequestHandler<LoginUserRequest, LoginUserResponse>
{
    public async Task<LoginUserResponse> Handle(LoginUserRequest request, CancellationToken cancellationToken)
    {
        var initialUser = await userRepository.FindByEmailAsync(request.Dto.Email)
            ?? throw new UnauthorizedAccessException("Błędny użytkownik lub hasło.");

        if (!initialUser.IsActive)
        {
            throw new UnauthorizedAccessException("Konto nie jest aktywne. Sprawdź e-mail, aby dokończyć rejestrację.");
        }

        var authenticatedUser = await userRepository.CheckPasswordAsync(initialUser, request.Dto.Password)
            ?? throw new UnauthorizedAccessException("Błędny użytkownik lub hasło.");

        var sessionId = Guid.NewGuid();
        var roles = await userRepository.GetRolesAsync(authenticatedUser, cancellationToken);

        var (accessToken, refreshToken) = tokenService.CreateToken(
            authenticatedUser.UserId,
            authenticatedUser.Email,
            authenticatedUser.CompanyName,
            authenticatedUser.Nip,
            roles,
            sessionId);

        var session = new UserSession
        {
            UserId = authenticatedUser.UserId,
            SessionId = sessionId,
            RefreshToken = refreshToken,
            LastActivityAt = DateTime.UtcNow,
            IsRevoked = false
        };

        await userRepository.AddSessionAsync(session, cancellationToken);

        return new LoginUserResponse(
            accessToken,
            !string.IsNullOrEmpty(accessToken),
            refreshToken,
            "Login successful");
    }
}