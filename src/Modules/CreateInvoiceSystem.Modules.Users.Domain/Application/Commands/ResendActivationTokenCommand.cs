using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.ResendToken;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;

public class ResendActivationTokenCommand : CommandBase<ResendActivationTokenRequest, ResendActivationTokenResponse, IUserRepository>
{
    private readonly IUserTokenService _userTokenService;
    private readonly IUserEmailSender _userEmailSender;
    private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;

    public ResendActivationTokenCommand(
        IUserTokenService userTokenService,
        IUserEmailSender userEmailSender,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _userTokenService = userTokenService;
        _userEmailSender = userEmailSender;
        _configuration = configuration;
    }

    public override async Task<ResendActivationTokenResponse> Execute(IUserRepository _userRepository, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(this.Parametr?.Email))
            return new ResendActivationTokenResponse { IsSuccess = false, Message = "Email jest wymagany." };

        var user = await _userRepository.FindByEmailAsync(this.Parametr.Email);
                
        if (user == null)
        {
            return new ResendActivationTokenResponse { IsSuccess = true, Message = "Jeśli konto istnieje i nie jest aktywne, nowy link został wysłany." };
        }

        if (user.IsActive)
        {
            return new ResendActivationTokenResponse { IsSuccess = false, Message = "To konto jest już aktywne." };
        }
        
        var token = _userTokenService.GenerateActivationToken(user.Email);
        var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/') ?? "https://localhost:7022";
        var activationLink = $"{frontendUrl}/activate?token={System.Uri.EscapeDataString(token)}";

        await _userEmailSender.SendActivationEmailAsync(user.Email, activationLink);

        return new ResendActivationTokenResponse
        {
            IsSuccess = true,
            Message = "Nowy link aktywacyjny został wysłany na Twój e-mail."
        };
    }
}