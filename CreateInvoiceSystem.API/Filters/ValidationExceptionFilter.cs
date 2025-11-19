using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using FluentValidation;


namespace CreateInvoiceSystem.API.Filters
{
    public class ValidationExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            if (context.Exception is ValidationException validationException)
            {
                var errors = validationException.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    message = e.ErrorMessage
                });
                context.Result = new JsonResult(new { errors })
                {
                    StatusCode = 400
                };
                context.ExceptionHandled = true;
            }
        }
    }
}
