namespace CreateInvoiceSystem.Products.Application.RequestsResponses.CreateProduct;

using CreateInvoiceSystem.Abstractions.Dto;
using MediatR;

public class CreateProductRequest(ProductDto productDto) : IRequest<CreateProductResponse>
{
    public ProductDto Product { get; } = productDto;
}
