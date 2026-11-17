using CreateInvoiceSystem.Abstractions.CQRS;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.RefreshToken
{
    public record RefreshTokenRequest(Guid RefreshToken) : IRequest<AuthResponse>, ITransactionalRequest;
}
