using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
public class CreateUserCommand : CommandBase<CreateUserDto, CreateUserDto, IUserRepository>
{
    public override async Task<CreateUserDto> Execute(IUserRepository _userRepository, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(_userRepository));
        if (this.Parametr.Address is null)
            throw new ArgumentNullException(nameof(this.Parametr.Address));
        
        var entity = UserMappers.ToEntity(this.Parametr);

        await _userRepository.AddAsync(entity, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);      

        var persisted = await _userRepository.GetUserByIdAsync(entity.UserId, cancellationToken);

        return persisted is not null
            ? persisted.ToCreateUserDto()
            : throw new InvalidOperationException("User was saved but could not be reloaded.");
    }
}
