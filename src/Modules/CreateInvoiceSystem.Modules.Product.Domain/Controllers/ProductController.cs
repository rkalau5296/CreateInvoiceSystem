using CreateInvoiceSystem.Abstractions.ControllerBase;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.CreateProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.DeleteProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProducts;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.UpdateProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CreateInvoiceSystem.Modules.Products.Domain.Controllers;


[ApiController]
[Route("[controller]")]
public class ProductController : ApiControllerBase
{
    public ProductController(IMediator mediator, ILogger<ProductController> logger) : base(mediator)
    {
        logger.LogInformation("This is ProductController");
    }

    [HttpGet("{ProductId}")]
    [ProducesResponseType(typeof(GetProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetProductAsync([FromRoute] int ProductId, CancellationToken cancellationToken)
    {
        GetProductRequest request = new(ProductId);
        return HandleRequest<GetProductRequest, GetProductResponse>(request, cancellationToken);
    }

    [HttpGet()]
    [Route("/Products")]
    public async Task<IActionResult> GetProductsAsync([FromQuery] GetProductsRequest request, CancellationToken cancellationToken)
    {
        return await HandleRequest<GetProductsRequest, GetProductsResponse>(request, cancellationToken);
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateProductsAsync([FromBody] CreateProductDto productDto, CancellationToken cancellationToken)
    {
        CreateProductRequest request = new(productDto);
        return await HandleRequest<CreateProductRequest, CreateProductResponse>(request, cancellationToken);
    }

    [HttpPut]
    [Route("update/id")]
    public async Task<IActionResult> UpdateProductAsync(int id, [FromBody] UpdateProductDto productDto, CancellationToken cancellationToken)
    {
        UpdateProductRequest request = new(id, productDto);
        return await HandleRequest<UpdateProductRequest, UpdateProductResponse>(request, cancellationToken);
    }

    [HttpDelete]
    [Route("id")]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        DeleteProductRequest request = new(id);
        return await HandleRequest<DeleteProductRequest, DeleteProductResponse>(request, cancellationToken);
    }
}
