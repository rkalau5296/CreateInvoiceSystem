namespace CreateInvoiceSystem.Products.Application.RequestsResponses.DeleteProduct;

using MediatR;

public class DeleteProductRequest(int id) : IRequest<DeleteProductResponse>
{
    public int Id { get; } =
        id >= 1 ? id
            : throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than or equal to 1.");
}
