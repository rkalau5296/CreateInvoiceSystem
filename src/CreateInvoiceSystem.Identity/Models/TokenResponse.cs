namespace CreateInvoiceSystem.Identity.Models;

public record TokenResponse(string AccessToken, Guid RefreshToken);
