namespace CreateInvoiceSystem.Modules.Products.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Modules.Products.Application.Queries;
using CreateInvoiceSystem.Modules.Products.Application.RequestsResponses.GetProduct;
using CreateInvoiceSystem.Modules.Products.Mappers;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

public class GetProductHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetProductRequest, GetProductResponse>
{
    public async Task<GetProductResponse> Handle(GetProductRequest request, CancellationToken cancellationToken)
    {
        GetProductQuery query = new(request.Id);
        var Product = await queryExecutor.Execute(query);      

        return new GetProductResponse
        {
            Data = ProductMappers.ToDto(Product),
        };
    }
}
