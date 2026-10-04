using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.UpdateUser;
using CreateInvoiceSystem.Modules.Users.Interfaces;
using CreateInvoiceSystem.Modules.Users.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Application.Handlers;

public class UpdateUserHandler(IUserRepository userRepository)
    : IRequestHandler<UpdateUserRequest, UpdateUserResponse>
{
    public async Task<UpdateUserResponse> Handle(
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var userDto = request.User;

        var user = await userRepository.GetUserByIdAsync(userDto.UserId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"User with ID {userDto.UserId} not found.");

        user.Name = userDto.Name ?? user.Name;
        user.CompanyName = userDto.CompanyName ?? user.CompanyName;
        user.Email = userDto.Email ?? user.Email;
        user.Nip = userDto.Nip ?? user.Nip;
        user.BankAccountNumber = userDto.BankAccountNumber ?? user.BankAccountNumber;

        if (userDto.Address is not null)
        {
            if (user.Address is null)
            {
                throw new InvalidOperationException(
                    $"Address for user {userDto.UserId} not found.");
            }

            user.Address.Street = userDto.Address.Street ?? user.Address.Street;
            user.Address.Number = userDto.Address.Number ?? user.Address.Number;
            user.Address.City = userDto.Address.City ?? user.Address.City;
            user.Address.PostalCode = userDto.Address.PostalCode ?? user.Address.PostalCode;
            user.Address.Country = userDto.Address.Country ?? user.Address.Country;
        }

        await userRepository.UpdateAsync(user, cancellationToken);

        return new UpdateUserResponse
        {
            Data = user.ToUpdateUserDto()
        };
    }
}