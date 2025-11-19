namespace CreateInvoiceSystem.Abstractions.ControllerBase;

using MediatR;
using Microsoft.AspNetCore.Mvc;
using CreateInvoiceSystem.Abstractions.ErrorResponseBase;
using System.Net;
using CreateInvoiceSystem.Abstractions.Error;

public abstract class ApiControllerBase(IMediator _mediator) : ControllerBase
{
    protected async Task<IActionResult> HandleRequest<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
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
                        errors = x.Value.Errors
                    }));
        }
        var response = await _mediator.Send(request, cancellationToken);

        if (response == null)
        {
            var notFoundError = new ErrorModel(ErrorType.NotFound);            
            return StatusCode((int)HttpStatusCode.NotFound, notFoundError);
        }

        if (response.Error != null)
        {
            return this.ErrorResponse(response.Error);
        }        

        return this.Ok(response);
    }

    private ObjectResult ErrorResponse(ErrorModel errorModel)
    {
        var httpCode = errorModel.Error switch
        {
            ErrorType.NotFound => HttpStatusCode.NotFound,
            ErrorType.InternalServerError => HttpStatusCode.InternalServerError,
            ErrorType.Unauthorized => HttpStatusCode.Unauthorized,
            ErrorType.RequestTooLarge => HttpStatusCode.RequestEntityTooLarge,
            ErrorType.TooManyRequests => (HttpStatusCode)429,
            ErrorType.UnsupportedMethod => HttpStatusCode.MethodNotAllowed,
            _ => HttpStatusCode.BadRequest,
        };

        return StatusCode((int)httpCode, errorModel);
    }
}
