using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;

public class RegisterUserCommand : CommandBase<RegisterUserDto, RegisterUserDto, IUserRepository>
{
    private readonly IUserEmailSender _userEmailSender;
    public RegisterUserCommand(IUserEmailSender userEmailSender) 
    {
        _userEmailSender = userEmailSender;
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

        await _userEmailSender.SendConfirmationRegistrationEmailAsync(this.Parametr.Email, "Rejestracja zakończona sukcesem", "Dziękujemy za rejestrację!");

        return entity.ToRegisterUserDto();
    }
}
