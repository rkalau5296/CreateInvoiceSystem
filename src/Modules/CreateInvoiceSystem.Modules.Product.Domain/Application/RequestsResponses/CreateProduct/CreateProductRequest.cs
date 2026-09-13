using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.CreateProduct;
public class CreateProductRequest(CreateProductDto productDto) : IRequest<CreateProductResponse>, ITransactionalRequest
{
    public CreateProductDto Product { get; } = productDto;

    [JsonIgnore]
    public int UserId { get; set; }
}
