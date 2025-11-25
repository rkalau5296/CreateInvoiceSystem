namespace CreateInvoiceSystem.Products.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Products.Application.Commands;
using CreateInvoiceSystem.Abstractions.Mappers;
using CreateInvoiceSystem.Products.Application.RequestsResponses.DeleteProduct;
using CreateInvoiceSystem.Abstractions.Entities;
using MediatR;

public class DeleteProductHandler(ICommandExecutor commandExecutor) : IRequestHandler<DeleteProductRequest, DeleteProductResponse>
{
    public async Task<DeleteProductResponse> Handle(DeleteProductRequest request, CancellationToken cancellationToken)
    {
        var Product = new Product { ProductId = request.Id };

        var command = new DeleteProductCommand { Parametr = Product };
        await commandExecutor.Execute(command, cancellationToken);

        return new DeleteProductResponse()
        {
            Data = ProductMappers.ToDto(Product)
        };
    }
}

