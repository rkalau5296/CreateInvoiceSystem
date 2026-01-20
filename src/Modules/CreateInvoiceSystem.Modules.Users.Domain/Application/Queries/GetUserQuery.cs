using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Queries;
public class GetUserQuery(int id) : QueryBase<User, IUserRepository>
{
    public override async Task<User> Execute(IUserRepository _userRepository, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetUserByIdAsync(id, cancellationToken)
            ?? throw new InvalidOperationException($"User with ID {id} not found.");
    }
}