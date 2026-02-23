using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace CreateInvoiceSystem.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex).ConfigureAwait(false);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception while processing request {Method} {Path}", context.Request?.Method, context.Request?.Path);

        context.Response.Clear();
        context.Response.ContentType = "application/json";

        ProblemDetails problem;
        int status;

        switch (exception)
        {
            case ValidationException fvEx:
                status = StatusCodes.Status400BadRequest;
                problem = new ValidationProblemDetails(fvEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key ?? string.Empty,
                        g => g.Select(e => e.ErrorMessage ?? string.Empty).Distinct().ToArray()
                    ))
                {
                    Title = "Validation failed",
                    Status = status,
                    Detail = _env.IsDevelopment() ? string.Join(" | ", fvEx.Errors.Select(e => e.ErrorMessage)) : null
                };
                break;

            case InvalidOperationException ioe:
                status = StatusCodes.Status404NotFound;
                problem = new ProblemDetails
                {
                    Title = "Invalid operation",
                    Status = status,
                    Detail = _env.IsDevelopment() ? ioe.Message : "Resource not found"
                };
                break;

            case ArgumentNullException ane:
                status = StatusCodes.Status400BadRequest;
                problem = new ProblemDetails
                {
                    Title = "Bad request",
                    Status = status,
                    Detail = _env.IsDevelopment() ? ane.Message : "Required value was null"
                };
                break;

            case ArgumentOutOfRangeException aore:
                status = StatusCodes.Status400BadRequest;
                problem = new ProblemDetails
                {
                    Title = "Bad request",
                    Status = status,
                    Detail = _env.IsDevelopment() ? aore.Message : "Argument out of range"
                };
                break;

            case DbUpdateException dbEx:
                status = StatusCodes.Status409Conflict;
                var inner = GetInnermostMessage(dbEx);
                problem = new ProblemDetails
                {
                    Title = "Database error",
                    Status = status,
                    Detail = _env.IsDevelopment() ? inner : "A database error occurred"
                };
                break;

            default:
                status = StatusCodes.Status500InternalServerError;
                problem = new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = status,
                    Detail = _env.IsDevelopment() ? exception.Message : "An unexpected error occurred"
                };
                break;
        }

        // optional: include trace id
        if (!string.IsNullOrEmpty(context.TraceIdentifier))
        {
            problem.Extensions["traceId"] = context.TraceIdentifier;
        }

        context.Response.StatusCode = status;
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, options)).ConfigureAwait(false);
    }

    private static string GetInnermostMessage(Exception ex)
    {
        while (ex.InnerException != null)
            ex = ex.InnerException;
        return ex.Message;
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}