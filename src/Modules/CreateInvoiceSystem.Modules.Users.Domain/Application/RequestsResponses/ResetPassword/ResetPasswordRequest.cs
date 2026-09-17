using CreateInvoiceSystem.Abstractions.CQRS;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ResetPassword;

public class ResetPasswordRequest : IRequest<ResetPasswordResponse>, ITransactionalRequest
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
