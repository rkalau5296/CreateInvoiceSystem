using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.DeleteProduct;
public class DeleteProductRequest(int id) : IRequest<DeleteProductResponse>
{
    public int Id { get; } =
        id >= 1 ? id
            : throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than or equal to 1.");
}
