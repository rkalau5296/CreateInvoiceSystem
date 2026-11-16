using CreateInvoiceSystem.Modules.Ksef.Dto;

namespace CreateInvoiceSystem.Modules.Ksef.Interfaces;

public interface IKsefCredentialsStore
{
    Task<bool> IsConfiguredAsync(int userId, CancellationToken cancellationToken);
    Task SaveAsync(int userId, string token, CancellationToken cancellationToken);
    Task DeleteAsync(int userId, CancellationToken cancellationToken);
    Task<KsefCredentials> GetRequiredAsync(int userId, CancellationToken cancellationToken);
}
