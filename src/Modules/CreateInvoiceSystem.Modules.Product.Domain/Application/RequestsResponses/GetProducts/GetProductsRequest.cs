using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProducts;
public class GetProductsRequest : IRequest<GetProductsResponse>
{
    public int? UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
