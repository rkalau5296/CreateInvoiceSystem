using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProducts;



public class GetProductsRequest : IRequest<GetProductsResponse>
{
    public int? UserId { get; set; }
}
