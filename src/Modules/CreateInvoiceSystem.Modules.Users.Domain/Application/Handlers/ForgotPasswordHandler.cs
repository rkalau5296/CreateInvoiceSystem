using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ForgotPassword;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;

public class ForgotPasswordHandler(IUserRepository _userRepository, IUserEmailSender emailSender) : IRequestHandler<ForgotPasswordRequest, ForgotPasswordResponse>
{
    public async Task<ForgotPasswordResponse> Handle(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var command = new ForgotPasswordCommand(request.Dto, emailSender);

        return await command.Execute(_userRepository, cancellationToken);
    }
}
