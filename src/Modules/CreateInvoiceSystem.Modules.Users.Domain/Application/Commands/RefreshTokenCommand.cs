using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RefreshToken;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands
{
    public class RefreshTokenCommand(
    RefreshTokenRequest request,
    IUserRepository _userRepository,
    IUserAuthService _userAuthService)
    {
        public async Task<AuthResponse> ExecuteAsync(CancellationToken cancellationToken)
        {
            var session = await _userRepository.GetSessionByTokenAsync(request.RefreshToken, cancellationToken);

            if (session is null || session.IsRevoked)
                throw new UnauthorizedAccessException("Sesja jest nieważna.");

            if (DateTime.UtcNow - session.LastActivityAt > TimeSpan.FromMinutes(5))
            {
                session.IsRevoked = true;
                await _userRepository.UpdateSessionAsync(session, cancellationToken);
                throw new UnauthorizedAccessException("Sesja wygasła z powodu bezczynności.");
            }

            var user = await _userRepository.GetUserByIdAsync(session.UserId, cancellationToken);
            if (user == null)
                throw new UnauthorizedAccessException("Użytkownik nie istnieje.");

            var authModel = new UserAuthModel(user.UserId, user.Email);

            var authResponse = _userAuthService.GenerateAuthResponse(authModel);

            session.RefreshToken = authResponse.RefreshToken;
            session.LastActivityAt = DateTime.UtcNow;

            await _userRepository.UpdateSessionAsync(session, cancellationToken);

            return authResponse;
        }
    }
}
