using CreateInvoiceSystem.Abstractions.ControllerBase;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ResendToken;

public class ResendActivationTokenResponse : IApiResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}
