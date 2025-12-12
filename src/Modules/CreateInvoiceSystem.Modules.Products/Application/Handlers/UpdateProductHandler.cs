using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Products.Application.Commands;
using CreateInvoiceSystem.Modules.Products.Application.RequestsResponses.UpdateProduct;
using CreateInvoiceSystem.Modules.Products.Services;
using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Application.Handlers;

public class UpdateProductHandler(ICommandExecutor commandExecutor, IInvoicePositionReadService invoicePosRead) : IRequestHandler<UpdateProductRequest, UpdateProductResponse>
{    
    public async Task<UpdateProductResponse> Handle(UpdateProductRequest request, CancellationToken cancellationToken)
    {        
        var command = new UpdateProductCommand(invoicePosRead) { Parametr = request.Product };
        
        var Product = await commandExecutor.Execute(command, cancellationToken);        

        return new UpdateProductResponse()
        {
            Data = Product
        };
    }
}
