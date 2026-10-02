using CreateInvoiceSystem.Abstractions.ControllerBase;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.CreateProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.DeleteProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProducts;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.UpdateProduct;
using CreateInvoiceSystem.Modules.Products.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CreateInvoiceSystem.Modules.Products.Domain.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductController : ApiControllerBase
{
    public ProductController(IMediator mediator, ILogger<ProductController> logger) : base(mediator)
    {
        logger.LogInformation("This is ProductController");
    }

    [HttpGet("{ProductId}")]
    [ProducesResponseType(typeof(GetProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductAsync([FromRoute] int ProductId, CancellationToken cancellationToken)
    {
        var claimValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();

        GetProductRequest request = new(ProductId) { UserId = actualUserId };
        return await HandleRequest<GetProductRequest, GetProductResponse>(request, cancellationToken);
    }

    [HttpGet]    
    public async Task<IActionResult> GetProductsAsync([FromQuery] GetProductsRequest request, CancellationToken cancellationToken)
    {
        var claimValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();

        request.UserId = actualUserId;

        return await HandleRequest<GetProductsRequest, GetProductsResponse>(request, cancellationToken);
    }

    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateProductsAsync([FromBody] CreateProductDto productDto, CancellationToken cancellationToken)
    {
        var claimValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();

        var secureDto = productDto with { UserId = actualUserId };

        CreateProductRequest request = new(secureDto) { UserId = actualUserId };
        return await HandleRequest<CreateProductRequest, CreateProductResponse>(request, cancellationToken);
    }

    [HttpPut]
    [Route("update/{id}")]
    public async Task<IActionResult> UpdateProductAsync(int id, [FromBody] UpdateProductDto productDto, CancellationToken cancellationToken)
    {
        var claimValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();

        var secureDto = productDto with { UserId = actualUserId };

        UpdateProductRequest request = new(id, secureDto) { UserId = actualUserId };
        return await HandleRequest<UpdateProductRequest, UpdateProductResponse>(request, cancellationToken);
    }

    [HttpDelete]
    [Route("{id}")]
    public async Task<IActionResult> DeleteProduct(int id, CancellationToken cancellationToken)
    {
        var claimValue = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(claimValue, out int actualUserId)) return Unauthorized();

        DeleteProductRequest request = new(id) { UserId = actualUserId };
        return await HandleRequest<DeleteProductRequest, DeleteProductResponse>(request, cancellationToken);
    }
}
