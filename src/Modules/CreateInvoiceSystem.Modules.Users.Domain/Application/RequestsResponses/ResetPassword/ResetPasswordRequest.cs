using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ResetPassword;

public class ResetPasswordRequest : IRequest<ResetPasswordResponse>
{
    public string Email { get; set; }
    public string Token { get; set; }
    public string NewPassword { get; set; }
}
