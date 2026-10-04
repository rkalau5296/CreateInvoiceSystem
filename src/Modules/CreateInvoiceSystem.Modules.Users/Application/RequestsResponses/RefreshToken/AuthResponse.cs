namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.RefreshToken;

public record AuthResponse(string AccessToken, Guid RefreshToken);
