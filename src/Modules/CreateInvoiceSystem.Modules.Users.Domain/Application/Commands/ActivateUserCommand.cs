namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ActivateUser;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

public class ActivateUserCommand : CommandBase<ActivateUserRequest, ActivateUserResponse, IUserRepository>
{
    private readonly IUserTokenService _userTokenService;

    public ActivateUserCommand(IUserTokenService userTokenService)
    {
        _userTokenService = userTokenService;
    }

    public override async Task<ActivateUserResponse> Execute(IUserRepository _userRepository, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null || string.IsNullOrEmpty(this.Parametr.Token))
        {
            return new ActivateUserResponse { IsSuccess = false, Message = "Brak tokena aktywacyjnego." };
        }
        
        var email = _userTokenService.GetEmailFromActivationToken(this.Parametr.Token);

        if (string.IsNullOrEmpty(email))
        {
            return new ActivateUserResponse { IsSuccess = false, Message = "Link wygasł lub jest nieprawidłowy." };
        }
        
        var user = await _userRepository.FindByEmailAsync(email);

        if (user == null)
        {
            return new ActivateUserResponse { IsSuccess = false, Message = "Użytkownik nie istnieje." };
        }

        if (user.IsActive)
        {
            return new ActivateUserResponse { IsSuccess = true, Message = "Konto jest już aktywne." };
        }
        
        user.IsActive = true;
        
        await _userRepository.UpdateAsync(user, cancellationToken);
        

        return new ActivateUserResponse
        {
            IsSuccess = true,
            Message = "Konto zostało aktywowane!"
        };
    }
}
