using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Products.Application.RequestsResponses.GetProduct;

public class GetProductRequest(int id) : IRequest<GetProductResponse>
{
    public int Id { get; set; } = id;

    [JsonIgnore]
    public int? UserId { get; set; }   
}