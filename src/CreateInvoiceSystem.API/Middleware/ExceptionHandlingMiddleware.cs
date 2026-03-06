using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.RegularExpressions;

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
                    Detail = string.Join(" | ", fvEx.Errors.Select(e => e.ErrorMessage))
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
                    Detail = msg
                };
                break;

            case UnauthorizedAccessException uaEx:
                status = StatusCodes.Status401Unauthorized;
                problem = new ProblemDetails
                {
                    Title = "Unauthorized",
                    Status = status,
                    Detail = uaEx.Message
                };
                break;
            case InvalidOperationException ioe:
                status = StatusCodes.Status404NotFound;
                problem = new ProblemDetails
                {
                    Title = "Invalid operation",
                    Status = status,
                    Detail = ioe.Message
                };
                break;

            case ArgumentNullException ane:
                status = StatusCodes.Status400BadRequest;
                problem = new ProblemDetails
                {
                    Title = "Bad request",
                    Status = status,
                    Detail = ane.Message
                };
                break;

            case ArgumentOutOfRangeException aore:
                status = StatusCodes.Status400BadRequest;
                problem = new ProblemDetails
                {
                    Title = "Bad request",
                    Status = status,
                    Detail = aore.Message
                };
                break;

            case DbUpdateException dbEx:
                {
                    var innermost = GetInnermostException(dbEx);

                    if (innermost is SqlException sqlEx)
                    {
                        if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                        {
                            var indexName = ExtractIndexNameFromSqlMessage(sqlEx.Message);
                            var indexMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["IX_Clients_Nip_UserId"] = "Istnieje już kontrahent o tym nr NIP"
                            };

                            if (!string.IsNullOrEmpty(indexName) && indexMap.TryGetValue(indexName, out var userMsg))
                            {
                                status = StatusCodes.Status409Conflict;
                                problem = new ProblemDetails
                                {
                                    Title = "Conflict",
                                    Status = status,
                                    Detail = userMsg
                                };
                                break;
                            }

                            status = StatusCodes.Status409Conflict;
                            problem = new ProblemDetails
                            {
                                Title = "Conflict",
                                Status = status,
                                Detail = sqlEx.Message
                            };
                            break;
                        }

                        status = StatusCodes.Status500InternalServerError;
                        problem = new ProblemDetails
                        {
                            Title = "Database error",
                            Status = status,
                            Detail = sqlEx.Message
                        };
                        break;
                    }

                    status = StatusCodes.Status409Conflict;
                    var inner = GetInnermostMessage(dbEx);
                    problem = new ProblemDetails
                    {
                        Title = "Database error",
                        Status = status,
                        Detail = inner
                    };
                }
                break;

            default:
                status = StatusCodes.Status500InternalServerError;
                problem = new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = status,
                    Detail = exception.Message
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
                var fallback = $"{{\"title\":\"{EscapeForJson(problem.Title)}\",\"status\":{status},\"detail\":\"{EscapeForJson(exception.Message)}\",\"traceId\":\"{EscapeForJson(context.TraceIdentifier)}\"}}";
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
    private static Exception GetInnermostException(Exception ex)
    {
        if (ex == null) throw new ArgumentNullException(nameof(ex));
        while (ex.InnerException != null)
            ex = ex.InnerException;
        return ex;
    }

    private static string? ExtractIndexNameFromSqlMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return null;

        var m = Regex.Match(message, @"unique (?:index|constraint)\s+'(?<idx>[^']+)'", RegexOptions.IgnoreCase);
        if (m.Success)
            return m.Groups["idx"].Value;

        m = Regex.Match(message, @"with unique index\s+(?<idx>\S+)", RegexOptions.IgnoreCase);
        if (m.Success)
            return m.Groups["idx"].Value;

        return null;
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}