using System.Security.Cryptography;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Modules.Ksef.Dto;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using CreateInvoiceSystem.Modules.Ksef.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

namespace CreateInvoiceSystem.API.Repositories.KsefCredentialsRepository;

public sealed class KsefCredentialsRepository(IDbContext db, IDataProtectionProvider protectionProvider)
    : IKsefCredentialsStore
{
    public async Task<bool> IsConfiguredAsync(int userId, CancellationToken cancellationToken)
    {
        return await (from credential in db.Set<KsefCredentialEntity>().AsNoTracking()
                      join user in db.Set<UserEntity>().AsNoTracking() on credential.UserId equals user.Id
                      where user.Id == userId && user.Nip == credential.Nip
                      select credential.UserId).AnyAsync(cancellationToken);
    }

    public async Task SaveAsync(int userId, string token, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token) || token.Length > 4096 || token.Any(char.IsWhiteSpace))
            throw new ArgumentException("Podaj poprawny token KSeF (bez spacji, maksymalnie 4096 znaków).");

        var nip = await GetNipAsync(userId, cancellationToken);
        var encrypted = Protector(userId).Protect(token);
        var entity = await db.Set<KsefCredentialEntity>()
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (entity is null)
        {
            entity = new KsefCredentialEntity { UserId = userId };
            db.Set<KsefCredentialEntity>().Add(entity);
        }

        entity.Nip = nip;
        entity.ProtectedToken = encrypted;
    }

    public async Task DeleteAsync(int userId, CancellationToken cancellationToken)
    {
        var entity = await db.Set<KsefCredentialEntity>()
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (entity is not null)
        {
            db.Set<KsefCredentialEntity>().Remove(entity);
        }
    }

    public async Task<KsefCredentials> GetRequiredAsync(int userId, CancellationToken cancellationToken)
    {
        var nip = await GetNipAsync(userId, cancellationToken);
        var entity = await db.Set<KsefCredentialEntity>().AsNoTracking()
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        if (entity is null || !string.Equals(entity.Nip, nip, StringComparison.Ordinal))
            throw new InvalidOperationException("Skonfiguruj token KSeF dla aktualnego NIP w ustawieniach konta.");

        try
        {
            return new KsefCredentials(nip, Protector(userId).Unprotect(entity.ProtectedToken));
        }
        catch (CryptographicException ex)
        {
            throw new InvalidOperationException("Nie można odszyfrować tokenu KSeF. Skontaktuj się z administratorem.", ex);
        }
    }

    private async Task<string> GetNipAsync(int userId, CancellationToken cancellationToken)
    {
        var nip = await db.Set<UserEntity>().AsNoTracking()
            .Where(user => user.Id == userId)
            .Select(user => user.Nip)
            .SingleOrDefaultAsync(cancellationToken);

        if (nip is null || nip.Length != 10 || nip.Any(character => character is < '0' or > '9'))
            throw new InvalidOperationException("Uzupełnij poprawny NIP w ustawieniach konta przed konfiguracją KSeF.");
        return nip;
    }

    private IDataProtector Protector(int userId) =>
        protectionProvider.CreateProtector("CreateInvoiceSystem.Ksef.Token.v1", userId.ToString(System.Globalization.CultureInfo.InvariantCulture));
}
