using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Products.Application.Commands;
using CreateInvoiceSystem.Modules.Products.Application.RequestsResponses.DeleteProduct;
using CreateInvoiceSystem.Modules.Products.Entities;
using CreateInvoiceSystem.Modules.Products.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Application.Handlers;

public class DeleteProductHandler(ICommandExecutor commandExecutor) : IRequestHandler<DeleteProductRequest, DeleteProductResponse>
{
    public async Task<DeleteProductResponse> Handle(DeleteProductRequest request, CancellationToken cancellationToken)
    {
        var Product = new Product { ProductId = request.Id };

        var command = new DeleteProductCommand() { Parametr = Product };
        await commandExecutor.Execute(command, cancellationToken);

        return new DeleteProductResponse()
        {
            Data = ProductMappers.ToDto(Product)
        };
    }
}

