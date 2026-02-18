using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ResendToken;

public class ResendActivationTokenRequest : IRequest<ResendActivationTokenResponse>
{
    public string Email { get; set; } = string.Empty;
}
