using MediatR;

namespace CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceXml;

public sealed class GetKsefInvoiceXmlRequest(string ksefNumber) : IRequest<string>
{
    public string KsefNumber { get; } = ksefNumber;
    public int UserId { get; set; }
}
