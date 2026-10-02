namespace CreateInvoiceSystem.Modules.Users.Domain.Dto;

public record UserTokenResult(string AccessToken, Guid RefreshToken);
