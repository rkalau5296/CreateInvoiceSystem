namespace CreateInvoiceSystem.API.Middleware;

public class ValidationExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (FluentValidation.ValidationException ex)
        {            
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors.Select(e => new
            {
                field = e.PropertyName,
                message = e.ErrorMessage
            });

            await context.Response.WriteAsJsonAsync(new { errors });
        }
    }
}