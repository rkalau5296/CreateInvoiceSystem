using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.ResetPassword;
using CreateInvoiceSystem.Modules.Users.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Application.Handlers;

public class ResetPasswordHandler(IUserRepository userRepository)
    : IRequestHandler<ResetPasswordRequest, ResetPasswordResponse>
{
    public async Task<ResetPasswordResponse> Handle(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {       
        var user = await userRepository.FindByEmailAsync(request.Email)
                   ?? throw new InvalidOperationException("Użytkownik nie istnieje.");

        var result = await userRepository.ResetPasswordAsync(
            user,
            request.Token,
            request.Version,
            request.NewPassword,
            cancellationToken);

        return new ResetPasswordResponse
        {
            IsSuccess = result,
            Message = result
                ? "Hasło zostało pomyślnie zmienione."
                : "Link do resetu hasła jest nieprawidłowy lub wygasł."
        };
    }
}