using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
public class UpdateUserCommand : CommandBase<UpdateUserDto, UpdateUserDto, IUserRepository>
{
    public override async Task<UpdateUserDto> Execute(IUserRepository _userRepository, CancellationToken cancellationToken = default)
    {
        if (Parametr is null)
            throw new ArgumentNullException(nameof(_userRepository));

        var user = await _userRepository.GetUserByIdAsync(Parametr.UserId, cancellationToken) ?? throw new InvalidOperationException($"User with ID {Parametr.UserId} not found.");            
        
        string oldName = user.Name;
        string oldCompanyName = user.CompanyName;
        string oldEmail = user.Email;
        string oldPassword = user.Password;
        string oldNip = user.Nip;
        string oldStreet = user.Address?.Street;
        string oldNumber = user.Address?.Number;
        string oldCity = user.Address?.City;
        string oldPostal = user.Address?.PostalCode;
        string oldCountry = user.Address?.Country;

        user.Name = Parametr.Name ?? user.Name;
        user.CompanyName = Parametr.CompanyName ?? user.CompanyName;
        user.Email = Parametr.Email ?? user.Email;
        user.Password = Parametr.Password ?? user.Password;
        user.Nip = Parametr.Nip ?? user.Nip;

        user.Address.Street = Parametr.Address?.Street ?? user.Address.Street;
        user.Address.Number = Parametr.Address?.Number ?? user.Address.Number;
        user.Address.City = Parametr.Address?.City ?? user.Address.City;
        user.Address.PostalCode = Parametr.Address?.PostalCode ?? user.Address.PostalCode;
        user.Address.Country = Parametr.Address?.Country ?? user.Address.Country;

        await _userRepository.UpdateAsync(user, cancellationToken);
        await _userRepository.SaveChangesAsync(cancellationToken);

        var persisted = await _userRepository.GetUserByIdAsync(user.UserId, cancellationToken);

        bool hasChanged = persisted is not null && (
            !string.Equals(oldName, persisted.Name, StringComparison.Ordinal) ||
            !string.Equals(oldCompanyName, persisted.CompanyName, StringComparison.Ordinal) ||
            !string.Equals(oldEmail, persisted.Email, StringComparison.Ordinal) ||
            !string.Equals(oldPassword, persisted.Password, StringComparison.Ordinal) ||
            !string.Equals(oldNip, persisted.Nip, StringComparison.Ordinal) ||
            !string.Equals(oldStreet, persisted.Address?.Street, StringComparison.Ordinal) ||
            !string.Equals(oldNumber, persisted.Address?.Number, StringComparison.Ordinal) ||
            !string.Equals(oldCity, persisted.Address?.City, StringComparison.Ordinal) ||
            !string.Equals(oldPostal, persisted.Address?.PostalCode, StringComparison.Ordinal) ||
            !string.Equals(oldCountry, persisted.Address?.Country, StringComparison.Ordinal)
        );

        return hasChanged
            ? persisted!.ToUpdateUserDto()
            : throw new InvalidOperationException($"No changes were saved for user with ID {Parametr.UserId}.");
    }
}
