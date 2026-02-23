using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using FluentValidation;

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
            // Ensure we catch and attempt to handle any exception
            await HandleExceptionAsync(context, ex).ConfigureAwait(false);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "Unhandled exception while processing request {Method} {Path}", context.Request?.Method, context.Request?.Path);

        if (context.Response.HasStarted)
        {
            _logger.LogWarning("Response has already started for {Path}. Cannot write ProblemDetails. TraceId={TraceId}", context.Request?.Path, context.TraceIdentifier);
            return;
        }

        ProblemDetails problem;
        int status;

        switch (exception)
        {
            case FluentValidation.ValidationException fvEx:
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

            case System.ComponentModel.DataAnnotations.ValidationException sysValEx:
                status = StatusCodes.Status400BadRequest;
                var msg = sysValEx.ValidationResult?.ErrorMessage ?? sysValEx.Message ?? "Validation failed";
                problem = new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    { string.Empty, new[] { msg } }
                })
                {
                    Title = "Validation failed",
                    Status = status,
                    Detail = _env.IsDevelopment() ? msg : null
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

        if (!string.IsNullOrEmpty(context.TraceIdentifier))
        {
            problem.Extensions["traceId"] = context.TraceIdentifier;
        }

        context.Response.Clear();
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        try
        {
            _logger.LogInformation("Writing ProblemDetails response for {Path} with status {Status}. TraceId={TraceId}", context.Request?.Path, status, context.TraceIdentifier);
            var json = JsonSerializer.Serialize(problem, options);
            await context.Response.WriteAsync(json).ConfigureAwait(false);
        }
        catch (Exception writeEx)
        {
            _logger.LogError(writeEx, "Failed to write ProblemDetails JSON for {Path}. TraceId={TraceId}", context.Request?.Path, context.TraceIdentifier);

            try
            {
                var fallback = _env.IsDevelopment()
                    ? $"{{\"title\":\"{EscapeForJson(problem.Title)}\",\"status\":{status},\"detail\":\"{EscapeForJson(exception.Message)}\",\"traceId\":\"{EscapeForJson(context.TraceIdentifier)}\"}}"
                    : $"{{\"title\":\"{EscapeForJson(problem.Title)}\",\"status\":{status},\"detail\":\"An unexpected error occurred\",\"traceId\":\"{EscapeForJson(context.TraceIdentifier)}\"}}";

                await context.Response.WriteAsync(fallback).ConfigureAwait(false);
            }
            catch (Exception finalEx)
            {
                _logger.LogError(finalEx, "Failed to write fallback error response for {Path}. TraceId={TraceId}", context.Request?.Path, context.TraceIdentifier);
            }
        }
    }

    private static string GetInnermostMessage(Exception ex)
    {
        while (ex.InnerException != null)
            ex = ex.InnerException;
        return ex.Message;
    }

    private static string EscapeForJson(string? s)
    {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}