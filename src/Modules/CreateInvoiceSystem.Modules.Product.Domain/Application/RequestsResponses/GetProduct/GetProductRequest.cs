using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProduct;
public class GetProductRequest : IRequest<GetProductResponse>
{
    private int _id;

    public int Id
    {
        get => _id;
        set => _id = value >= 1 ? value
               : throw new ArgumentOutOfRangeException(nameof(Id), "Id must be greater than or equal to 1.");
    }
    
    public int? UserId { get; set; }

    public GetProductRequest(int id)
    {
        Id = id;
    }    
    public GetProductRequest() { }
}
