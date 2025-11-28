namespace CreateInvoiceSystem.Users.Application.Queries;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Entities;
using Microsoft.EntityFrameworkCore;

public class GetUserQuery(int id) : QueryBase<User>
{
    public override async Task<User> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        return await context.Set<User>()
            .Include(u => u.Address)
            .FirstOrDefaultAsync(c => c.UserId == id, cancellationToken: cancellationToken) 
            ?? throw new InvalidOperationException($"User with ID {id} not found.");
    }
}