using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ResetPassword;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Abstractions.CQRS;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;

public class ResetPasswordCommand : CommandBase<ResetPasswordRequest, ResetPasswordResponse, IUserRepository>
{
    public override async Task<ResetPasswordResponse> Execute(IUserRepository userRepository, CancellationToken cancellationToken = default)
    {        
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(this.Parametr), "Parametr (Request) nie został ustawiony w komendzie!");

        var user = await userRepository.FindByEmailAsync(this.Parametr.Email)
                   ?? throw new InvalidOperationException("Użytkownik nie istnieje.");

        var result = await userRepository.ResetPasswordAsync(
            user,
            this.Parametr.Token,
            this.Parametr.NewPassword,
            cancellationToken);

        var response = new ResetPasswordResponse
        {
            IsSuccess = result,
            Message = result ? "Hasło zostało pomyślnie zmienione." : "Błąd resetowania hasła."
        };

        return response;
    }
}