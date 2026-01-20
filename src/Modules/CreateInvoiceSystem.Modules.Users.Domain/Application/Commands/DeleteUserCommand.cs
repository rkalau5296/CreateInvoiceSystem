using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Mappers;


namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
public class DeleteUserCommand : CommandBase<User, UserDto, IUserRepository>
{
    public override async Task<UserDto> Execute(IUserRepository _userRepository, CancellationToken cancellationToken = default)
    {
        if (Parametr is null)
            throw new ArgumentNullException(nameof(_userRepository));

        var userEntity = await _userRepository.GetUserByIdAsync(Parametr.UserId, cancellationToken) ?? throw new InvalidOperationException($"User with ID {Parametr.UserId} not found.");

        if (userEntity.Invoices.Any() || userEntity.Clients.Any() || userEntity.Products.Any())
            throw new InvalidOperationException($"Cannot delete User with ID {Parametr.UserId} because it has associated Invoices.");

        await _userRepository.RemoveAsync(userEntity, cancellationToken);
        if (userEntity.Address is not null)
            await _userRepository.RemoveAddress(userEntity.Address, cancellationToken);

        await _userRepository.SaveChangesAsync(cancellationToken);        

        bool addrExists = await _userRepository.IsAddressExists(userEntity.Address.AddressId, cancellationToken);
        bool userExists = await _userRepository.IsUserExists(userEntity.UserId, cancellationToken);

        return !userExists && !addrExists
            ? UserMappers.ToDto(userEntity)
            : throw new InvalidOperationException($"Failed to delete User or User address with ID {Parametr.UserId}.");
    }
}