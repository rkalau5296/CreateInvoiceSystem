using CreateInvoiceSystem.Modules.Products.Dto;
using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Application.RequestsResponses.UpdateProduct;

public class UpdateProductRequest(int id, UpdateProductDto productDto) : IRequest<UpdateProductResponse>
{
    public UpdateProductDto Product { get; } =
        (productDto ?? throw new ArgumentNullException(nameof(productDto),
            $"Argument '{nameof(productDto)}' for product update request (Id={id}) cannot be null. Make sure the request body contains all required fields."
        )) with { ProductId = id };
    public int Id { get; } =
    id >= 1 ? id
        : throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than or equal to 1.");
}
