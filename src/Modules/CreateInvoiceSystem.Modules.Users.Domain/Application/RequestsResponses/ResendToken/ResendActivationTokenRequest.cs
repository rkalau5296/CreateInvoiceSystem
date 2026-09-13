using CreateInvoiceSystem.Abstractions.CQRS;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ResendToken;

public class ResendActivationTokenRequest : IRequest<ResendActivationTokenResponse>, ITransactionalRequest
{
    public string Email { get; set; } = string.Empty;
}
