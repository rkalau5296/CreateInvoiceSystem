using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.RefreshToken
{
    public record RefreshTokenRequest(Guid RefreshToken) : IRequest<AuthResponse>;
}
