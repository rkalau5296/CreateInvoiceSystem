namespace CreateInvoiceSystem.Modules.Products.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using MediatR;
using CreateInvoiceSystem.Modules.Products.Application.RequestsResponses.GetProducts;
using CreateInvoiceSystem.Modules.Products.Application.Queries;
using CreateInvoiceSystem.Modules.Products.Entities;
using CreateInvoiceSystem.Modules.Products.Mappers;

public class GetProductsHandler(IQueryExecutor queryExecutor) : IRequestHandler<GetProductsRequest, GetProductsResponse>
{
    public async Task<GetProductsResponse> Handle(GetProductsRequest request, CancellationToken cancellationToken)
    {
        GetProductsQuery query = new();

        List<Product> Products = await queryExecutor.Execute(query);

        return new GetProductsResponse
        {
            Data = ProductMappers.ToDtoList(Products)
        }; 
    }
}