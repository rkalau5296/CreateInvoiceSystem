namespace CreateInvoiceSystem.Identity.Models;

public record IdentityUserModel(
int UserId,
string Email,
string CompanyName,
string Nip,
List<string> Roles);
