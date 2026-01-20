using CreateInvoiceSystem.Identity.Interfaces;
using CreateInvoiceSystem.Identity.Models;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.API.Adapters.UserTokenAdapter
{
    public class UserTokenAdapter(IJwtProvider jwtProvider) : IUserTokenService
    {
        public (string AccessToken, Guid RefreshToken) CreateToken(int userId, string email, string company, string nip, List<string> roles)
        {
            var model = new IdentityUserModel(userId, email, company, nip, roles);
            
            var identityResult = jwtProvider.Generate(model);
            
            return (identityResult.AccessToken, identityResult.RefreshToken);
        }
    }
}
