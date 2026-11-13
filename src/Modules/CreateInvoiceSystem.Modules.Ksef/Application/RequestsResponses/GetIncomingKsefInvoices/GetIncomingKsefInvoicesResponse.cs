using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Ksef.Dto;

namespace CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetIncomingKsefInvoices;

public sealed class GetIncomingKsefInvoicesResponse : ResponseBase<List<KsefInvoiceDto>>
{
    public bool HasMore { get; set; }
}
