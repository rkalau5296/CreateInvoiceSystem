using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ChangePassword;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;

public class ChangePasswordHandler(IUserRepository _userRepository) : IRequestHandler<ChangePasswordRequest, ChangePasswordResponse>
{
    public async Task<ChangePasswordResponse> Handle(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (request.Dto.NewPassword != request.Dto.ConfirmPassword)
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
            request.Dto.OldPassword,
            request.Dto.NewPassword
        );

        return new ChangePasswordResponse(Succeeded, ErrorMessage);
    }
}
