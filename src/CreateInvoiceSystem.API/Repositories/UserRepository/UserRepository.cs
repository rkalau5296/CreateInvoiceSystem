using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.API.Mappers.UserMapper;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Entities;
using CreateInvoiceSystem.Modules.Clients.Persistence.Entities;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Entities;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Entities;
using CreateInvoiceSystem.Modules.Products.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Entities;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CreateInvoiceSystem.API.Repositories.UserRepository;

public class UserRepository : IUserRepository
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IDbContext _db;
    private readonly UserManager<UserEntity> _userManager;
    private readonly ILookupNormalizer _normalizer;

    public UserRepository(IDbContext db, UserManager<UserEntity> userManager, IHttpContextAccessor httpContextAccessor, ILookupNormalizer normalizer)
    {
        _db = db;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _normalizer = normalizer;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        var addressEntity = UserMapper.ToAddressEntity(user.Address);
        await _db.Set<AddressEntity>().AddAsync(addressEntity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        user.AddressId = addressEntity.AddressId;

        var userEntity = UserMapper.ToUserEntity(user);
        await _db.Set<UserEntity>().AddAsync(userEntity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IdentityResult> CreateWithPasswordAsync(User user, string password)
    {
        var addressEntity = UserMapper.ToAddressEntity(user.Address);
        await _db.Set<AddressEntity>().AddAsync(addressEntity);
        await _db.SaveChangesAsync();

        user.AddressId = addressEntity.AddressId;

        var userEntity = UserMapper.ToUserEntity(user);
        userEntity.UserName = user.Email;
        userEntity.NormalizedUserName = _normalizer.NormalizeName(user.Email);
        userEntity.NormalizedEmail = _normalizer.NormalizeEmail(user.Email);
        userEntity.BankAccountNumber = user.BankAccountNumber;

        user.Name = user.Email;

        return await _userManager.CreateAsync(userEntity, password);
    }

    public async Task<User> GetUserByIdAsync(int userId, CancellationToken cancellationToken)
    {
        var baseData = await (from user in _db.Set<UserEntity>().AsNoTracking()
                              join addr in _db.Set<AddressEntity>().AsNoTracking()
                              on user.AddressId equals addr.AddressId
                              where user.Id == userId
                              select new { user, addr })
                          .SingleOrDefaultAsync(cancellationToken);

        if (baseData == null) return null;

        var invoices = await _db.Set<InvoiceEntity>()
            .Where(i => i.UserId == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var invoiceIds = invoices.Select(i => i.InvoiceId).ToList();
        var invoicePositions = await _db.Set<InvoicePositionEntity>()
            .Where(ip => invoiceIds.Contains(ip.InvoiceId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var products = await _db.Set<ProductEntity>()
            .Where(p => p.UserId == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var clients = await _db.Set<ClientEntity>()
            .Where(c => c.UserId == userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var clientAddressIds = clients.Select(c => c.AddressId).ToList();
        var clientAddresses = await _db.Set<AddressEntity>()
            .Where(a => clientAddressIds.Contains(a.AddressId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return UserMapper.MapFull(baseData.user, baseData.addr, invoices, invoicePositions, products, clients, clientAddresses);
    }

    public async Task<List<User>> GetUsersAsync(CancellationToken cancellationToken)
    {
        var userBase = await (from user in _db.Set<UserEntity>().AsNoTracking()
                              join addr in _db.Set<AddressEntity>().AsNoTracking()
                              on user.AddressId equals addr.AddressId
                              select new { user, addr })
                          .ToListAsync(cancellationToken);

        var allInvoices = await _db.Set<InvoiceEntity>().AsNoTracking().ToListAsync(cancellationToken);
        var allProducts = await _db.Set<ProductEntity>().AsNoTracking().ToListAsync(cancellationToken);
        var allClients = await _db.Set<ClientEntity>().AsNoTracking().ToListAsync(cancellationToken);
        var allClientAddresses = await _db.Set<AddressEntity>().AsNoTracking().ToListAsync(cancellationToken);
        var allInvoicePositions = await _db.Set<InvoicePositionEntity>().AsNoTracking().ToListAsync(cancellationToken);

        return userBase.Select(x => UserMapper.MapSummary(x.user, x.addr, allInvoices, allInvoicePositions, allProducts, allClients, allClientAddresses)).ToList();
    }

    public async Task<bool> IsAddressExists(int addressId, CancellationToken cancellationToken)
    {
        return await _db.Set<AddressEntity>()
            .AsNoTracking()
            .AnyAsync(a => a.AddressId == addressId, cancellationToken);
    }

    public async Task<bool> IsUserExists(int userId, CancellationToken cancellationToken)
    {
        return await _db.Set<UserEntity>()
            .AsNoTracking()
            .AnyAsync(c => c.Id == userId, cancellationToken);
    }

    public async Task RemoveAddress(int addressId, CancellationToken cancellationToken)
    {
        var addressEntity = await _db.Set<AddressEntity>().FirstOrDefaultAsync(a => a.AddressId == addressId, cancellationToken);
        if (addressEntity != null)
        {
            _db.Set<AddressEntity>().Remove(addressEntity);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RemoveAsync(int userId, CancellationToken ct)
    {
        var user = await _db.Set<UserEntity>().FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user != null)
        {
            _db.Set<UserEntity>().Remove(user);
            await _db.SaveChangesAsync(ct);
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken)
    {
        var userEntity = await _db.Set<UserEntity>()
            .SingleOrDefaultAsync(u => u.Id == user.UserId, cancellationToken);

        if (userEntity == null) return;

        var addressEntity = await _db.Set<AddressEntity>()
            .SingleOrDefaultAsync(a => a.AddressId == user.AddressId, cancellationToken);

        userEntity.Name = user.Name;
        userEntity.CompanyName = user.CompanyName;
        userEntity.Nip = user.Nip;
        userEntity.BankAccountNumber = user.BankAccountNumber;

        var newEmail = user.Email;
        if (!string.IsNullOrWhiteSpace(newEmail))
        {
            var oldEmail = userEntity.Email;
            if (!string.Equals(oldEmail, newEmail, StringComparison.OrdinalIgnoreCase))
            {
                var normalizedEmail = _normalizer.NormalizeEmail(newEmail);
                var normalizedUserName = _normalizer.NormalizeName(newEmail);

                userEntity.Email = newEmail;
                userEntity.NormalizedEmail = normalizedEmail;
                userEntity.UserName = newEmail;
                userEntity.NormalizedUserName = normalizedUserName;
            }
            else
            {
                userEntity.NormalizedEmail ??= _normalizer.NormalizeEmail(userEntity.Email ?? string.Empty);
                userEntity.NormalizedUserName ??= _normalizer.NormalizeName(userEntity.UserName ?? string.Empty);
            }
        }

        if (user.Address != null)
        {
            if (addressEntity == null)
            {                
                var newAddress = UserMapper.ToAddressEntity(user.Address);
                await _db.Set<AddressEntity>().AddAsync(newAddress, cancellationToken);
                await _db.SaveChangesAsync(cancellationToken);
                userEntity.AddressId = newAddress.AddressId;
            }
            else
            {             
                addressEntity.Street = user.Address.Street;
                addressEntity.Number = user.Address.Number;
                addressEntity.City = user.Address.City;
                addressEntity.PostalCode = user.Address.PostalCode;
                addressEntity.Country = user.Address.Country;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<User> CheckPasswordAsync(User user, string password)
    {
        var userEntity = await _userManager.FindByIdAsync(user.UserId.ToString());
        if (userEntity == null) return null;

        var valid = await _userManager.CheckPasswordAsync(userEntity, password);
        return valid ? UserMapper.MapLight(userEntity) : null;
    }

    public async Task<User> FindByEmailAsync(string email)
    {
        var userEntity = await _userManager.FindByEmailAsync(email);
        return userEntity == null ? null : UserMapper.MapLight(userEntity);
    }

    public async Task<List<string>> GetRolesAsync(User user, CancellationToken cancellationToken)
    {
        var identityUser = await _userManager.FindByIdAsync(user.UserId.ToString());

        if (identityUser == null) return new List<string>();

        var roles = await _userManager.GetRolesAsync(identityUser);

        return roles.ToList();
    }

    public async Task<(string Token, string Version)?> GeneratePasswordResetTokenAsync(
    User user,
    CancellationToken cancellationToken)
    {
        var identityUser = await _userManager.FindByIdAsync(user.UserId.ToString());

        if (identityUser is null)
            return null;

        identityUser.PasswordResetVersion = Guid.NewGuid().ToString("N");

        var updateResult = await _userManager.UpdateAsync(identityUser);

        if (!updateResult.Succeeded)
            return null;

        var token = await _userManager.GeneratePasswordResetTokenAsync(identityUser);

        return (token, identityUser.PasswordResetVersion!);
    }


    public async Task<bool> ResetPasswordAsync(
    User user,
    string token,
    string version,
    string newPassword,
    CancellationToken cancellationToken)
    {
        var identityUser = await _userManager.FindByIdAsync(user.UserId.ToString());

        if (identityUser is null)
            return false;

        if (string.IsNullOrWhiteSpace(identityUser.PasswordResetVersion))
            return false;

        if (identityUser.PasswordResetVersion != version)
            return false;

        var result = await _userManager.ResetPasswordAsync(identityUser, token, newPassword);

        if (!result.Succeeded)
            return false;

        identityUser.PasswordResetVersion = Guid.NewGuid().ToString("N");

        var updateResult = await _userManager.UpdateAsync(identityUser);

        return updateResult.Succeeded;
    }


    public async Task AddSessionAsync(UserSession session, CancellationToken cancellationToken)
    {
        var entity = UserMapper.ToUserSessionEntity(session);
        await _db.Set<UserSessionEntity>().AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        session.Id = entity.Id;
    }

    public async Task UpdateSessionActivityAsync(UserSession session, CancellationToken cancellationToken)
    {
        var entity = await _db.Set<UserSessionEntity>()
            .FirstOrDefaultAsync(s => s.RefreshToken == session.RefreshToken, cancellationToken);

        if (entity is not null)
        {
            entity.LastActivityAt = DateTime.UtcNow;
            session.LastActivityAt = entity.LastActivityAt;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateSessionAsync(UserSession session, CancellationToken cancellationToken)
    {
        var entity = await _db.Set<UserSessionEntity>()
            .FirstOrDefaultAsync(s => s.Id == session.Id, cancellationToken);

        if (entity is not null)
        {
            entity.RefreshToken = session.RefreshToken;
            entity.LastActivityAt = session.LastActivityAt;
            entity.IsRevoked = session.IsRevoked;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<UserSession?> GetSessionByTokenAsync(Guid refreshToken, CancellationToken cancellationToken)
    {
        var entity = await _db.Set<UserSessionEntity>()
            .FirstOrDefaultAsync(s => s.RefreshToken == refreshToken, cancellationToken);

        return UserMapper.ToUserSession(entity);
    }

    public async Task<int> GetLoggedUserId(CancellationToken cancellationToken = default)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return 0;
        }

        return await Task.FromResult(userId);
    }

    public async Task<(bool Succeeded, string ErrorMessage)> ChangePasswordAsync(User user, string oldPassword, string newPassword)
    {
        var userEntity = await _userManager.FindByIdAsync(user.UserId.ToString());

        if (userEntity == null)
        {
            return (false, "Nie odnaleziono profilu użytkownika w systemie Identity.");
        }

        var result = await _userManager.ChangePasswordAsync(userEntity, oldPassword, newPassword);

        if (result.Succeeded)
        {
            return (true, null);
        }

        var error = result.Errors.FirstOrDefault()?.Description ?? "Błąd podczas zmiany hasła.";
        return (false, error);
    }

    public async Task<int> RemoveInactiveUsersAsync(DateTime cutoffDate, CancellationToken ct)
    {
        var query = _db.Set<UserEntity>()
        .Where(u => !u.IsActive && u.CreatedAt < cutoffDate)
        .Where(u =>
            !_db.Set<InvoiceEntity>().Any(i => i.UserId == u.Id) &&
            !_db.Set<ProductEntity>().Any(p => p.UserId == u.Id) &&
            !_db.Set<ClientEntity>().Any(c => c.UserId == u.Id)
        );

        var usersToRemove = await query.ToListAsync(ct);

        var count = usersToRemove.Count;

        if (count > 0)
        {
            _db.Set<UserEntity>().RemoveRange(usersToRemove);
            await _db.SaveChangesAsync(ct);
        }

        return count;
    }

    public async Task<List<User>> GetUsersForCleanupWarningAsync(DateTime warningDate, CancellationToken ct)
    {
        var dayStart = warningDate.Date;
        var dayEnd = dayStart.AddDays(1);

        var entities = await _db.Set<UserEntity>()
            .Where(u => !u.IsActive
                     && u.CreatedAt >= dayStart
                     && u.CreatedAt < dayEnd)
            .ToListAsync(ct);

        return entities.Select(UserMapper.MapLight).ToList();
    }

    public async Task SaveActivationTokenJtiAsync(int userId, string jti, DateTimeOffset expiryUtc, CancellationToken ct = default)
    {
        var user = await _db.Set<UserEntity>().FindAsync(new object[] { userId }, ct) ?? throw new InvalidOperationException("User not found");
        user.ActivationTokenJti = jti;
        user.ActivationTokenExpiry = expiryUtc;

        await _db.SaveChangesAsync(ct);
    }

    public async Task<bool> ValidateAndActivateUserByTokenAsync(string email, string tokenJti, DateTimeOffset? tokenExpiry, CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        var user = await _db.Set<UserEntity>()
            .FirstOrDefaultAsync(u =>
                u.Email == email &&
                !u.IsActive &&
                !string.IsNullOrWhiteSpace(u.ActivationTokenJti) &&
                u.ActivationTokenJti == tokenJti &&
                (u.ActivationTokenExpiry == null || u.ActivationTokenExpiry >= now),
                ct);

        if (user == null)
            return false;

        if (tokenExpiry.HasValue && tokenExpiry.Value < now)
            return false;

        user.IsActive = true;
        user.ActivationTokenJti = null;
        user.ActivationTokenExpiry = null;

        await _db.SaveChangesAsync(ct);

        return true;
    }
}