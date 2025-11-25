namespace CreateInvoiceSystem.Products.Application.Handlers;

using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Products.Application.Queries;
using CreateInvoiceSystem.Products.Application.RequestsResponses.GetProducts;
using MediatR;
using CreateInvoiceSystem.Abstractions.Mappers;

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