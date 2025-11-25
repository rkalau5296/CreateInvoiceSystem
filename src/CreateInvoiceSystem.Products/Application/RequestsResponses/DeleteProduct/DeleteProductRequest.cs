namespace CreateInvoiceSystem.Products.Application.RequestsResponses.DeleteProduct;

using MediatR;

public class DeleteProductRequest(int id) : IRequest<DeleteProductResponse>
{
    public int Id { get; } = id;
}
