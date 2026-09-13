using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Mappers;


namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
public class DeleteUserCommand : CommandBase<User, UserDto, IUserRepository>
{
    public override async Task<UserDto> Execute(IUserRepository userRepository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(Parametr);

        var user = await userRepository.GetUserByIdAsync(
            Parametr.UserId,
            cancellationToken)
            ?? throw new InvalidOperationException(
                $"User with ID {Parametr.UserId} not found.");

        if (user.Invoices.Any()
            || user.Clients.Any()
            || user.Products.Any())
        {
            throw new InvalidOperationException(
                $"Cannot delete User with ID {Parametr.UserId} "
                + "because it has associated data.");
        }

        await userRepository.RemoveAsync(Parametr.UserId, cancellationToken);

        if (user.Address is not null)
        {
            await userRepository.RemoveAddress(user.AddressId, cancellationToken);
        }

        return UserMappers.ToDto(user);
    }
}