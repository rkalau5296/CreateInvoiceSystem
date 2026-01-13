using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;

public class LoginUserCommand : CommandBase<LoginUserDto, string, IUserRepository>
{
    private readonly IUserTokenService _tokenService;
    
    public LoginUserCommand(LoginUserDto dto, IUserTokenService tokenService)
    {
        this.Parametr = dto;
        _tokenService = tokenService;
    }

    public override async Task<string> Execute(IUserRepository _userRepository, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(this.Parametr));

        var initialUser = await _userRepository.FindByEmailAsync(this.Parametr.Email) ?? throw new UnauthorizedAccessException("Invalid e-mail or password.");

        var authenticatedUser = await _userRepository.CheckPasswordAsync(initialUser, this.Parametr.Password) ?? throw new UnauthorizedAccessException("Invalid e-mail or password.");

        var roles = await _userRepository.GetRolesAsync(authenticatedUser, cancellationToken);

        return _tokenService.CreateToken(
        authenticatedUser.UserId,
        authenticatedUser.Email,
        authenticatedUser.CompanyName,
        authenticatedUser.Nip,
        roles);
    }
}
