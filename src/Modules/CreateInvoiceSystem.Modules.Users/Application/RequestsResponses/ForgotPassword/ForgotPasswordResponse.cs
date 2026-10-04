using CreateInvoiceSystem.Abstractions.ControllerBase;

namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.ForgotPassword;

public record ForgotPasswordResponse(bool IsSuccess, string Message, string? ResetToken = null) : IApiResponse;
