using CreateInvoiceSystem.Identity.Interfaces;
using CreateInvoiceSystem.Identity.Models;
using CreateInvoiceSystem.Identity.Services;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.API.Adapters.UserTokenAdapter
{
    public class UserTokenAdapter(IJwtProvider _jwtProvider) : IUserTokenService
    {
        public (string AccessToken, Guid RefreshToken) CreateToken(int userId, string email, string company, string nip, List<string> roles)
        {
            var model = new IdentityUserModel(userId, email, company, nip, roles);
            
            var identityResult = _jwtProvider.Generate(model);
            
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
    }
}
