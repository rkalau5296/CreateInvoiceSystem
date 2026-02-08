namespace CreateInvoiceSystem.Frontend.Models;

public record UserDto(
int UserId,
string Name,
string CompanyName,
string Email,
string Password,
string Nip,
AddressDto Address,
string BankAccountNumber,

IEnumerable<object>? Invoices,
IEnumerable<object>? Clients,
IEnumerable<object>? Products
);
