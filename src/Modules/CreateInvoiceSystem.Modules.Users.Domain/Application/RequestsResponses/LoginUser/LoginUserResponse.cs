namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.LoginUser;

public record LoginUserResponse(string Token, bool IsSuccess, Guid RefreshToken, string Message = "");