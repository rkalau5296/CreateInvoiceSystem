using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.RefreshToken;
using CreateInvoiceSystem.Modules.Users.Entities;
using CreateInvoiceSystem.Modules.Users.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Application.Handlers;

public class RefreshTokenHandler(
    IUserRepository _userRepository,
    IUserAuthService _userAuthService) : IRequestHandler<RefreshTokenRequest, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var session = await _userRepository.GetSessionByTokenAsync(request.RefreshToken, cancellationToken);

        if (session is null || session.IsRevoked)
            throw new UnauthorizedAccessException("Sesja jest nieważna.");

        if (DateTime.UtcNow - session.LastActivityAt > TimeSpan.FromMinutes(30))
        {
            session.IsRevoked = true;
            await _userRepository.UpdateSessionAsync(session, cancellationToken);
            throw new UnauthorizedAccessException("Sesja wygasła z powodu bezczynności.");
        }

        var user = await _userRepository.GetUserByIdAsync(session.UserId, cancellationToken);
        if (user == null)
            throw new UnauthorizedAccessException("Użytkownik nie istnieje.");

        var authModel = new UserAuthModel(user.UserId, user.Email);

        var authResponse = _userAuthService.GenerateAuthResponse(authModel, session.SessionId);

        session.RefreshToken = authResponse.RefreshToken;
        session.LastActivityAt = DateTime.UtcNow;

        await _userRepository.UpdateSessionAsync(session, cancellationToken);

        return authResponse;
    }
}
