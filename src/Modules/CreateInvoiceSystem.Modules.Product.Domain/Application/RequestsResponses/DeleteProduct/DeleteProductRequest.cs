using CreateInvoiceSystem.Abstractions.CQRS;
using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.DeleteProduct;

public class DeleteProductRequest(int id) : IRequest<DeleteProductResponse>, ITransactionalRequest
{
    public int Id { get; } = id;

    [JsonIgnore]
    public int UserId { get; set; }
}