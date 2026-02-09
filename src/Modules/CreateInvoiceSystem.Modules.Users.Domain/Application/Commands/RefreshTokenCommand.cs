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

            if (DateTime.UtcNow - session.LastActivityAt > TimeSpan.FromMinutes(120))
            {
                session.IsRevoked = true;
                await _userRepository.UpdateSessionActivityAsync(session, cancellationToken);
                throw new UnauthorizedAccessException("Sesja wygasła z powodu bezczynności.");
            }

            await _userRepository.UpdateSessionActivityAsync(session, cancellationToken);

            var user = await _userRepository.GetUserByIdAsync(session.UserId, cancellationToken);

            var authModel = new UserAuthModel(
                    user.UserId,
                    user.Email
                );            

            return _userAuthService.GenerateAuthResponse(authModel);
        }
    }
}
