using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ForgotPassword;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;

public class ForgotPasswordCommand : CommandBase<ForgotPasswordDto, ForgotPasswordResponse, IUserRepository>
{
    private readonly IUserEmailSender _userEmailSender;
    
    public ForgotPasswordCommand(ForgotPasswordDto dto, IUserEmailSender userEmailSender)
    {
        this.Parametr = dto;
        _userEmailSender = userEmailSender;
    }

    public override async Task<ForgotPasswordResponse> Execute(IUserRepository _userRepository, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(this.Parametr));
        
        var user = await _userRepository.FindByEmailAsync(this.Parametr.Email);
        
        if (user is not null)
        {
            var token = await _userRepository.GeneratePasswordResetTokenAsync(user, cancellationToken);

            await _userEmailSender.SendResetPasswordEmailAsync(user.Email, token);
        }
       
        return new ForgotPasswordResponse(true, "If your email is in our database, you will receive a reset link.");
    }
}
