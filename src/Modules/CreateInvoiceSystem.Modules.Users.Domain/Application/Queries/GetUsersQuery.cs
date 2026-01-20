using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Queries;
public class GetUsersQuery : QueryBase<List<User>, IUserRepository>
{
    public override async Task<List<User>> Execute(IUserRepository _userRepository, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetUsersAsync(cancellationToken)
            ?? throw new InvalidOperationException($"List of users is empty.");
    }
}
