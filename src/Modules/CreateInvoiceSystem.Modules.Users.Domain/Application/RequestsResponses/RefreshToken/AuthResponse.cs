namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RefreshToken;

public record AuthResponse(
string AccessToken,
Guid RefreshToken
);
