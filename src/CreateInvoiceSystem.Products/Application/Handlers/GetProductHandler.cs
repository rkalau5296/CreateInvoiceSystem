namespace CreateInvoiceSystem.Products.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Abstractions.Mappers;
using CreateInvoiceSystem.Products.Application.Queries;
using CreateInvoiceSystem.Products.Application.RequestsResponses.GetProduct;
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
