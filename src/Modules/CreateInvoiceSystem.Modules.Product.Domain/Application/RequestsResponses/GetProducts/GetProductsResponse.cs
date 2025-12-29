using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProducts;
public class GetProductsResponse : ResponseBase<List<ProductDto>>
{    
}