using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoice;
public class GetInvoiceRequest : IRequest<GetInvoiceResponse>
{
    private int _id;
    public int Id
    {
        get => _id;
        set => _id = value >= 1 ? value
               : throw new ArgumentOutOfRangeException(nameof(Id), "Id must be greater than or equal to 1.");
    }

    public int? UserId { get; set; }

    public GetInvoiceRequest(int id)
    {
        Id = id;
    }

    public GetInvoiceRequest(int? userId, int id)
    {
        Id = id;
        UserId = userId;
    }

    public GetInvoiceRequest() { }
}
