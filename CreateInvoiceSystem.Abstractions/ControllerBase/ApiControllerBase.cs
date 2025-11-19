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

        return this.Ok(response);
    }    
}
