namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ResendToken;

public class ResendActivationTokenResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
}
