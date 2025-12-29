using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.DeleteProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Entities;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Commands;
using MediatR;
using CreateInvoiceSystem.Modules.Products.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;

public class DeleteProductHandler(ICommandExecutor commandExecutor, IProductRepository _productRepository) : IRequestHandler<DeleteProductRequest, DeleteProductResponse>
{
    public async Task<DeleteProductResponse> Handle(DeleteProductRequest request, CancellationToken cancellationToken)
    {
        var Product = new Product { ProductId = request.Id };

        var command = new DeleteProductCommand() { Parametr = Product };
        await commandExecutor.Execute(command, _productRepository, cancellationToken);

        return new DeleteProductResponse()
        {
            Data = ProductMappers.ToDto(Product)
        };
    }
}

