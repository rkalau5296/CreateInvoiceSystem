using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.API.Repositories.UserRepository;

public class UserRepository : IUserRepository
{    
    private readonly IDbContext _db;  
    
    public UserRepository(IDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(User entity, CancellationToken cancellationToken)
    {        
        await _db.Set<User>().AddAsync(entity, cancellationToken).AsTask();
    }

    public async Task<User> GetUserByIdAsync(int userId, CancellationToken cancellationToken)
    {
        return await _db.Set<User>().AsNoTracking()
            .Include(c => c.Address)
            .SingleOrDefaultAsync(u => u.UserId == userId, cancellationToken);        
    }

    public async Task<List<User>> GetUsersAsync(CancellationToken cancellationToken)
    {
        return await _db.Set<User>()
            .AsNoTracking()
            .Include(c => c.Address)
            .ToListAsync(cancellationToken);        
    }

    public async Task<bool> IsAddressExists(int addressId, CancellationToken cancellationToken)
    {
        return await _db.Set<Address>()
            .AsNoTracking()
            .AnyAsync(a => a.AddressId == addressId, cancellationToken);
    }

    public async Task<bool> IsUserExists(int userId, CancellationToken cancellationToken)
    {
        return await _db.Set<User>()
            .AsNoTracking()
            .AnyAsync(c => c.UserId == userId, cancellationToken);
    }

    public async Task RemoveAddress(Address address, CancellationToken cancellationToken)
    {
        _db.Set<Address>().Remove(address);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(User user, CancellationToken cancellationToken)
    {        
        _db.Set<User>().Remove(user);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }    
}
