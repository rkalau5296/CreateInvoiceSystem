namespace CreateInvoiceSystem.Modules.Ksef.Persistence.Entities;

public sealed class KsefCredentialEntity
{
    public int UserId { get; set; }
    public string Nip { get; set; } = string.Empty;
    public string ProtectedToken { get; set; } = string.Empty;
}
