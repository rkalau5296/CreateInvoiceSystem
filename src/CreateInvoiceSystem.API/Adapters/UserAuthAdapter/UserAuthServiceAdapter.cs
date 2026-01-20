using CreateInvoiceSystem.Identity.Interfaces;
using CreateInvoiceSystem.Identity.Models;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RefreshToken;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.API.Adapters.UserAuthAdapter;

public class UserAuthServiceAdapter(IJwtProvider _jwtProvider) : IUserAuthService
{
    public AuthResponse GenerateAuthResponse(UserAuthModel user)
    {        
        var identityUser = new IdentityUserModel(
            user.Id,
            user.Email,
            string.Empty, 
            string.Empty, 
            new List<string>()
        );

        var tokenResponse = _jwtProvider.Generate(identityUser);

        return new AuthResponse(tokenResponse.AccessToken, tokenResponse.RefreshToken);
    }
}
