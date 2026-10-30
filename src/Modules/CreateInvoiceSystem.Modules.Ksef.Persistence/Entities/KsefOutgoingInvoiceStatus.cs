namespace CreateInvoiceSystem.Modules.Ksef.Persistence.Entities;

public enum KsefInvoiceStatus
{
    Wysylanie = 0,
    OczekujeNaPotwierdzenie = 1,
    Zaakceptowana = 2,
    Odrzucona = 3
}

public class KsefOutgoingInvoiceStatus
{
    public int KsefOutgoingInvoiceStatusId { get; set; }
    public int InvoiceId { get; set; }
    public int UserId { get; set; }
    public KsefInvoiceStatus Status { get; set; }        
    public string? ElementReferenceNumber { get; set; }        
    public string? KsefReferenceNumber { get; set; }
    public string? UpoXml { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime SentAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }    
}