using CreateInvoiceSystem.Identity.Models;

namespace CreateInvoiceSystem.Identity.Interfaces;

public interface IJwtProvider
{
    string Generate(IdentityUserModel userModel);
}
