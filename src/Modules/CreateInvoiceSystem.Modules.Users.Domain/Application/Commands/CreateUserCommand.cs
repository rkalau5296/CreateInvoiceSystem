using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
public class CreateUserCommand : CommandBase<CreateUserDto, CreateUserDto, IUserRepository>
{
    public override async Task<CreateUserDto> Execute(IUserRepository userRepository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(Parametr);
        ArgumentNullException.ThrowIfNull(Parametr.Address);

        var user = UserMappers.ToEntity(Parametr);

        await userRepository.AddAsync(user, cancellationToken);

        return user.ToCreateUserDto();
    }
}
