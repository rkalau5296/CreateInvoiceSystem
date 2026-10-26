namespace CreateInvoiceSystem.Modules.Ksef.Application.Options;

public class KsefApiOptions
{
    public string ContextIdentifierNip { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api-test.ksef.mf.gov.pl";
    public string KsefToken { get; set; } = string.Empty;
}