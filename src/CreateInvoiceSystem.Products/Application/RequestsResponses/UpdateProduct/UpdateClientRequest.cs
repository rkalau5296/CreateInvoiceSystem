namespace CreateInvoiceSystem.Products.Application.RequestsResponses.UpdateProduct;

using CreateInvoiceSystem.Abstractions.Dto;
using MediatR;

public class UpdateProductRequest(int id, ProductDto productDto) : IRequest<UpdateProductResponse>
{
    public ProductDto Client { get; } = productDto with { ProductId = id };
    public int Id { get; set; } = id;

}
