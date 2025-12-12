namespace CreateInvoiceSystem.Modules.Products.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Products.Application.Commands;
using CreateInvoiceSystem.Modules.Products.Application.RequestsResponses.CreateProduct;
using MediatR;

public class CreateProductHandler(ICommandExecutor commandExecutor) : IRequestHandler<CreateProductRequest, CreateProductResponse>
{   
    public async Task<CreateProductResponse> Handle(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateProductCommand() { Parametr = request.Product };

        var ProductFromDb = await commandExecutor.Execute(command, cancellationToken);

        return new CreateProductResponse()
        {
            Data = ProductFromDb
        };
    }
}