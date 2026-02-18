using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Mappers;
using Microsoft.Extensions.Configuration;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;

public class RegisterUserCommand : CommandBase<RegisterUserDto, RegisterUserDto, IUserRepository>
{
    private readonly IUserEmailSender _userEmailSender;
    private readonly IUserTokenService _userTokenService;
    private readonly IConfiguration _configuration;

    public RegisterUserCommand(
        IUserEmailSender userEmailSender,
        IUserTokenService userTokenService,
        IConfiguration configuration)
    {
        _userEmailSender = userEmailSender;
        _userTokenService = userTokenService;
        _configuration = configuration;
    }

    public override async Task<RegisterUserDto> Execute(IUserRepository _userRepository, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(this.Parametr));
        
        var entity = UserMappers.ToEntity(this.Parametr);

        var result = await _userRepository.CreateWithPasswordAsync(entity, this.Parametr.Password);

        if (!result.Succeeded)
        {        
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Rejestracja nieudana: {errors}");
        }

        var token = _userTokenService.GenerateActivationToken(entity.Email);
       
        var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/') ?? "https://localhost:7022";
        var activationLink = $"{frontendUrl}/activate?token={Uri.EscapeDataString(token)}";
        
        await _userEmailSender.SendActivationEmailAsync(entity.Email, activationLink);

        return entity.ToRegisterUserDto();
    }
}
