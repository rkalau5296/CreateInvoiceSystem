namespace CreateInvoiceSystem.Modules.Products.Application.RequestsResponses.CreateProduct;

using CreateInvoiceSystem.Modules.Products.Dto;
using MediatR;

public class CreateProductRequest(CreateProductDto productDto) : IRequest<CreateProductResponse>
{
    public CreateProductDto Product { get; } = productDto;
}
