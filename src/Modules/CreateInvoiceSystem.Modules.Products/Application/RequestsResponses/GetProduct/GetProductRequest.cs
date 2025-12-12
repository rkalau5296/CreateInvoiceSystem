namespace CreateInvoiceSystem.Modules.Products.Application.RequestsResponses.GetProduct;

using MediatR;

public class GetProductRequest(int id) : IRequest<GetProductResponse>
{
    public int Id { get; set; } = id >= 1 ? id
            : throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than or equal to 1.");
}
