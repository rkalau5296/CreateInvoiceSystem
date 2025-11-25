namespace CreateInvoiceSystem.Products.Application.RequestsResponses.GetProduct;

using MediatR;

public class GetProductRequest(int id) : IRequest<GetProductResponse>
{
    public int Id { get; set; } = id;
}
