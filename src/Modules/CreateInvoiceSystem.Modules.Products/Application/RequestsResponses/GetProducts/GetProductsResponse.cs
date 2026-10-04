using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Products.Dto;

namespace CreateInvoiceSystem.Modules.Products.Application.RequestsResponses.GetProducts;
public class GetProductsResponse : ResponseBase<List<ProductDto>>
{
    public int TotalCount { get; set; }
}