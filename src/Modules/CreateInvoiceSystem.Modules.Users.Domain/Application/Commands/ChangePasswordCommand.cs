using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ChangePassword;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;

public class ChangePasswordCommand : CommandBase<ChangePasswordDto, ChangePasswordResponse, IUserRepository>
{
    public override async Task<ChangePasswordResponse> Execute(IUserRepository _userRepository, CancellationToken cancellationToken = default)
    {
        if (Parametr.NewPassword != Parametr.ConfirmPassword)
        {
            return new ChangePasswordResponse(false, "Nowe hasło i potwierdzenie nie są zgodne.");
        }

        var userId = await _userRepository.GetLoggedUserId(cancellationToken);
        if (userId == 0)
            return new ChangePasswordResponse(false, "Nieautoryzowany dostęp.");

        var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user == null)
            return new ChangePasswordResponse(false, "Użytkownik nie istnieje.");
     
        var (Succeeded, ErrorMessage) = await _userRepository.ChangePasswordAsync(
            user,
            Parametr.OldPassword,
            Parametr.NewPassword
        );
        
        return new ChangePasswordResponse(Succeeded, ErrorMessage);
    }
}