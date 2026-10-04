namespace CreateInvoiceSystem.Modules.Users.Dto;

public record UserTokenResult(string AccessToken, Guid RefreshToken);
