using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.CreateProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;
public class CreateProductHandler(ICommandExecutor commandExecutor, IProductRepository _productRepository) : IRequestHandler<CreateProductRequest, CreateProductResponse>
{   
    public async Task<CreateProductResponse> Handle(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand() { Parametr = request.Product };

        var ProductFromDb = await commandExecutor.Execute(command, _productRepository, cancellationToken);

        return new CreateProductResponse()
        {
            Data = ProductFromDb
        };
    }
}