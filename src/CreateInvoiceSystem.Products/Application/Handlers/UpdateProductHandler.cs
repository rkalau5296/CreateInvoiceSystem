namespace CreateInvoiceSystem.Products.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Products.Application.Commands;
using CreateInvoiceSystem.Products.Application.RequestsResponses.UpdateProduct;
using MediatR;


public class UpdateProductHandler(ICommandExecutor commandExecutor) : IRequestHandler<UpdateProductRequest, UpdateProductResponse>
{    
    public async Task<UpdateProductResponse> Handle(UpdateProductRequest request, CancellationToken cancellationToken)
    {        
        var command = new UpdateProductCommand() { Parametr = request.Product };
        
        var Product = await commandExecutor.Execute(command, cancellationToken);        

        return new UpdateProductResponse()
        {
            Data = Product
        };
    }
}
