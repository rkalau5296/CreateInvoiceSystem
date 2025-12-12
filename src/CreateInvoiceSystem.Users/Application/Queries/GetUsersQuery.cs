namespace CreateInvoiceSystem.Modules.Users.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Users.Entities;
using Microsoft.EntityFrameworkCore;

public class GetUsersQuery : QueryBase<List<User>>
{
    public override async Task<List<User>> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        return await context.Set<User>()
            .Include(u => u.Address)
            .ToListAsync(cancellationToken: cancellationToken) 
            ?? throw new InvalidOperationException($"List of addresses is empty.");
    }
}
