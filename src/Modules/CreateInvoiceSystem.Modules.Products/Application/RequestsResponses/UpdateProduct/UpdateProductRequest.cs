using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Products.Dto;
using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Products.Application.RequestsResponses.UpdateProduct;

public class UpdateProductRequest(int id, UpdateProductDto productDto) : IRequest<UpdateProductResponse>, ITransactionalRequest
{
    public int Id { get; } = id;
    public UpdateProductDto Product { get; } = productDto != null ? productDto with { ProductId = id } : null!;

    [JsonIgnore]
    public int UserId { get; set; }    
}
