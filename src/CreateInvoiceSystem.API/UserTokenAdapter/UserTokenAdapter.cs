using CreateInvoiceSystem.Identity.Interfaces;
using CreateInvoiceSystem.Identity.Models;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.API.UserTokenAdapter
{
    public class UserTokenAdapter(IJwtProvider jwtProvider) : IUserTokenService
    {
        public string CreateToken(int userId, string email, string company, string nip, List<string> roles)
        {            
            var model = new IdentityUserModel(userId, email, company, nip, roles);
            return jwtProvider.Generate(model);
        }
    }
}
