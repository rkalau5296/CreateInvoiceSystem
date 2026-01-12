using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;

namespace CreateInvoiceSystem.Modules.Users.Domain.Interfaces;

public interface IUserRepository : ISaveChangesContext
{    
    Task<User> GetUserByIdAsync(int userId, CancellationToken cancellationToken);    
    Task RemoveAsync(User user, CancellationToken cancellationToken);
    Task RemoveAddress(Address address, CancellationToken cancellationToken);
    Task<bool> IsUserExists(int userId, CancellationToken cancellationToken);
    Task<bool> IsAddressExists(int addressId, CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task<List<User>> GetUsersAsync(CancellationToken cancellationToken);
    Task UpdateAsync(User user, CancellationToken cancellationToken);
}
