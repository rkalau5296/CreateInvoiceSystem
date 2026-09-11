using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;

public class UpdateUserCommand : CommandBase<UpdateUserDto, UpdateUserDto, IUserRepository>
{
    public override async Task<UpdateUserDto> Execute(IUserRepository userRepository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(Parametr);

        var user = await userRepository.GetUserByIdAsync(Parametr.UserId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"User with ID {Parametr.UserId} not found.");

        user.Name = Parametr.Name ?? user.Name;
        user.CompanyName = Parametr.CompanyName ?? user.CompanyName;
        user.Email = Parametr.Email ?? user.Email;
        user.Nip = Parametr.Nip ?? user.Nip;
        user.BankAccountNumber = Parametr.BankAccountNumber ?? user.BankAccountNumber;

        if (Parametr.Address is not null)
        {
            if (user.Address is null)
            {
                throw new InvalidOperationException(
                    $"Address for user {Parametr.UserId} not found.");
            }

            user.Address.Street =
                Parametr.Address.Street ?? user.Address.Street;
            user.Address.Number =
                Parametr.Address.Number ?? user.Address.Number;
            user.Address.City =
                Parametr.Address.City ?? user.Address.City;
            user.Address.PostalCode =
                Parametr.Address.PostalCode ?? user.Address.PostalCode;
            user.Address.Country =
                Parametr.Address.Country ?? user.Address.Country;
        }

        await userRepository.UpdateAsync(user, cancellationToken);

        return user.ToUpdateUserDto();
    }
}