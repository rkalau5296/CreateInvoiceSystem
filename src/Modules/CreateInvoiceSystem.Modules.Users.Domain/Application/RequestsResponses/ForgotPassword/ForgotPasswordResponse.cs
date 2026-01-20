namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ForgotPassword;

public record ForgotPasswordResponse(bool IsSuccess, string Message, string? ResetToken = null);
