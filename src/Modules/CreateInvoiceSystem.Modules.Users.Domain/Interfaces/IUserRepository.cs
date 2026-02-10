using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using Microsoft.AspNetCore.Identity;

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
    Task<IdentityResult> CreateWithPasswordAsync(User user, string password);
    Task<User> CheckPasswordAsync(User user, string password);
    Task<User> FindByEmailAsync(string email);    
    Task<List<string>> GetRolesAsync(User user, CancellationToken cancellationToken);
    Task<string> GeneratePasswordResetTokenAsync(User user, CancellationToken cancellationToken);
    Task<bool> ResetPasswordAsync(User user, string token, string newPassword, CancellationToken cancellationToken);
    Task AddSessionAsync(UserSession session, CancellationToken cancellationToken);
    Task UpdateSessionActivityAsync(UserSession session, CancellationToken cancellationToken);
    public Task<UserSession?> GetSessionByTokenAsync(Guid refreshToken, CancellationToken cancellationToken);
    Task UpdateSessionAsync(UserSession session, CancellationToken cancellationToken);
}
