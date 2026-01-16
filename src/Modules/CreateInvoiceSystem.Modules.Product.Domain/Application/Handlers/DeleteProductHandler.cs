using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.DeleteProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Commands;
using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;

public class DeleteProductHandler(ICommandExecutor commandExecutor, IProductRepository _productRepository) : IRequestHandler<DeleteProductRequest, DeleteProductResponse>
{
    public async Task<DeleteProductResponse> Handle(DeleteProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product 
        { 
            ProductId = request.Id,
            UserId = request.UserId,
        };

        var command = new DeleteProductCommand() { Parametr = product };
        var deletedProduct = await commandExecutor.Execute(command, _productRepository, cancellationToken);

        return new DeleteProductResponse()
        {
            Data = deletedProduct
        };
    }
}

