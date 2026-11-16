namespace CreateInvoiceSystem.Modules.Ksef.Interfaces;

public interface IKsefInvoicePdfGenerator
{
    byte[] Create(byte[] xml, string ksefNumber, string verificationUrl);
}
