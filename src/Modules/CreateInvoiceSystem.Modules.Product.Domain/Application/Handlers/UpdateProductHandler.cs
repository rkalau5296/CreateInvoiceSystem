using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.UpdateProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Products.Domain.Application.Handlers;

public class UpdateProductHandler(ICommandExecutor commandExecutor, IProductRepository _productRepository) : IRequestHandler<UpdateProductRequest, UpdateProductResponse>
{    
    public async Task<UpdateProductResponse> Handle(UpdateProductRequest request, CancellationToken cancellationToken)
    {        
        var command = new UpdateProductCommand() { Parametr = request.Product };
        
        var Product = await commandExecutor.Execute(command, _productRepository, cancellationToken);        

        return new UpdateProductResponse()
        {
            Data = Product
        };
    }
}
