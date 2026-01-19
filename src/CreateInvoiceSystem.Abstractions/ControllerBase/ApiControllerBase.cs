namespace CreateInvoiceSystem.Abstractions.ControllerBase;

using MediatR;
using Microsoft.AspNetCore.Mvc;

public abstract class ApiControllerBase(IMediator _mediator) : ControllerBase
{
    protected async Task<IActionResult> HandleRequest<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
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

    protected async Task<IActionResult> HandleFileRequest<TRequest, TResponse>(
        TRequest request, string contentType, string fileName, Func<TResponse, byte[]> fileSelector, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        var response = await _mediator.Send(request, cancellationToken);
        if (response == null) return NotFound();

        return File(fileSelector(response), contentType, fileName);
    }
}
