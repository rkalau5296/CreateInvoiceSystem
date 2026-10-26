namespace CreateInvoiceSystem.Modules.Ksef.Interfaces;

public interface IKsefApiClient
{
    Task<string> AuthenticateAsync(CancellationToken cancellationToken);
}