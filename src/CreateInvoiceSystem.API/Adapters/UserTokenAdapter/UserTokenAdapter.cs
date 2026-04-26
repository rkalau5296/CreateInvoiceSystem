// src/CreateInvoiceSystem.API/Adapters/UserTokenAdapter/UserTokenAdapter.cs

using CreateInvoiceSystem.Identity.Interfaces;
using CreateInvoiceSystem.Identity.Models;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.API.Adapters.UserTokenAdapter
{
    public class UserTokenAdapter(IJwtProvider _jwtProvider) : IUserTokenService
    {
        public (string AccessToken, Guid RefreshToken) CreateToken(int userId, string email, string company, string nip, List<string> roles)
        {
            var refreshToken = Guid.NewGuid();
            var model = new IdentityUserModel(userId, email, company, nip, roles);
            var identityResult = _jwtProvider.Generate(model, refreshToken);

            return (identityResult.AccessToken, identityResult.RefreshToken);
        }

        public string GenerateActivationToken(string email)
        {
            return _jwtProvider.GenerateActivationToken(email, 24);
        }

        public string? GetEmailFromActivationToken(string token)
        {
            return _jwtProvider.GetEmailFromActivationToken(token);
        }

        public string GenerateActivationToken(string email, int expiresHours)
        {
            return _jwtProvider.GenerateActivationToken(email, expiresHours);
        }
    }
}