namespace CreateInvoiceSystem.Products.Application.RequestsResponses.CreateProduct;

using CreateInvoiceSystem.Abstractions.Dto;
using MediatR;

public class CreateProductRequest(CreateProductDto productDto) : IRequest<CreateProductResponse>
{
    public CreateProductDto Product { get; } = productDto;
}
