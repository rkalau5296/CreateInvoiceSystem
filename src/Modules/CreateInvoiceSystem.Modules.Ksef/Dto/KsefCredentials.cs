namespace CreateInvoiceSystem.Modules.Ksef.Dto;

public sealed class KsefCredentials(string nip, string token)
{
    public string Nip { get; } = nip;
    public string Token { get; } = token;
}
