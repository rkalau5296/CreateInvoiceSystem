using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.CreateProduct;




public class CreateProductRequest(CreateProductDto productDto) : IRequest<CreateProductResponse>
{
    public CreateProductDto Product { get; } = productDto;
}
