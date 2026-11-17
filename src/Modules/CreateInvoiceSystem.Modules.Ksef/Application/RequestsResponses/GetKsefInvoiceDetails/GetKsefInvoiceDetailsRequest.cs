using CreateInvoiceSystem.Modules.Ksef.Dto;
using MediatR;

namespace CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceDetails;

public sealed class GetKsefInvoiceDetailsRequest(string ksefNumber) : IRequest<KsefInvoiceDetailsDto>
{
    public string KsefNumber { get; } = ksefNumber;
    public int UserId { get; set; }
}
