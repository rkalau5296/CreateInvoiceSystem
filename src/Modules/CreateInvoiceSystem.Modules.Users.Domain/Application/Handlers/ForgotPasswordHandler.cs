using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ForgotPassword;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;

public class ForgotPasswordHandler(IUserRepository _userRepository, IUserEmailSender _emailSender) : IRequestHandler<ForgotPasswordRequest, ForgotPasswordResponse>
{
    public async Task<ForgotPasswordResponse> Handle(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.FindByEmailAsync(request.Dto.Email);

        if (user is not null)
        {
            var resetData = await _userRepository.GeneratePasswordResetTokenAsync(user, cancellationToken);
            if (resetData.HasValue)
            {
                var (token, version) = resetData.Value;
                await _emailSender.SendResetPasswordEmailAsync(user.Email, token, version);
            }
        }
        return new ForgotPasswordResponse(
            true,
            "If your email is in our database, you will receive a reset link.");
    }
}