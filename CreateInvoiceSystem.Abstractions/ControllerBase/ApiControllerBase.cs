namespace CreateInvoiceSystem.Abstractions.ControllerBase;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using CreateInvoiceSystem.Abstractions.ErrorResponseBase;
using System;
using System.Net;
using CreateInvoiceSystem.Abstractions.Error;

public abstract class ApiControllerBase(IMediator _mediator) : ControllerBase
{
    protected async Task<IActionResult> HandleRequest<TRequest, TResponse>(TRequest request)
        where TRequest : IRequest<TResponse>
        where TResponse : ErrorResponseBase
    {
        if (!this.ModelState.IsValid)
        {
            return this.BadRequest(
                this.ModelState.Where(x => x.Value.Errors.Any())
                    .Select(x => new
                    {
                        property = x.Key,
                        errors = x.Value.Errors}));
        }
        var response = await _mediator.Send(request);
        if (response.Error != null)
        {
            return this.ErrorResponse(response.Error);
        }

        return this.Ok(response);
    }

    private IActionResult ErrorResponse(ErrorModel errorModel)
    {
        var httpCode = GetHttpStatusCode(errorModel.Error);
        return StatusCode((int)httpCode, errorModel);
    }

    private static HttpStatusCode GetHttpStatusCode(string errorType)
    {
        return errorType switch
        {
            ErrorType.NotFound => HttpStatusCode.NotFound,
            ErrorType.InternalServerError => HttpStatusCode.InternalServerError,
            ErrorType.Unauthorized => HttpStatusCode.Unauthorized,
            ErrorType.RequestTooLarge => HttpStatusCode.RequestEntityTooLarge,
            ErrorType.TooManyRequests => (HttpStatusCode)429,// Too Many Requests
            ErrorType.UnsupportedMethod => HttpStatusCode.MethodNotAllowed,
            _ => HttpStatusCode.BadRequest,
        };
    }
}
