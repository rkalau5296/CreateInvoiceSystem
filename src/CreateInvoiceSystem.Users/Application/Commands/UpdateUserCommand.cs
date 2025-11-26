namespace CreateInvoiceSystem.Users.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;
using Microsoft.EntityFrameworkCore;

public class UpdateUserCommand : CommandBase<UserDto, UserDto>
{
    public override async Task<UserDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        var User = await context.Set<User>()            
            .FirstOrDefaultAsync(c => c.UserId == Parametr.UserId, cancellationToken: cancellationToken)            
            ?? throw new InvalidOperationException($"User with ID {Parametr.UserId} not found.");        

        User.Name = Parametr.Name;
        User.Email = Parametr.Email;
        User.Password = Parametr.Password;

        await context.SaveChangesAsync(cancellationToken);        
        return UserMappers.ToDto(User);
    }
}
