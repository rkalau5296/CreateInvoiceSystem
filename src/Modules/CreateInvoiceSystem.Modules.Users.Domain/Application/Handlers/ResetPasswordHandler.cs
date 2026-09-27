using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ResetPassword;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;

public class ResetPasswordHandler(IUserRepository userRepository)
    : IRequestHandler<ResetPasswordRequest, ResetPasswordResponse>
{
    public async Task<ResetPasswordResponse> Handle(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (request is null)
            throw new ArgumentNullException(nameof(request), "Request nie może być null.");

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