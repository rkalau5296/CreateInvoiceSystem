using System.Text.Json;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

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

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        ProblemDetails problem;
        int status;

        var innermost = GetInnermostException(exception);
        if (IsConnectionIssue(innermost))
        {
            status = StatusCodes.Status500InternalServerError;
            problem = new ProblemDetails
            {
                Title = "Database error",
                Status = status,
                Detail = "Nie można nawiązać połączenia z bazą danych."
            };

            if (!string.IsNullOrEmpty(context.TraceIdentifier))
            {
                problem.Extensions["traceId"] = context.TraceIdentifier;
            }

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = status;

            var json = JsonSerializer.Serialize(problem, jsonOptions);
            await context.Response.WriteAsync(json).ConfigureAwait(false);
            return;
        }

        switch (exception)
        {
            case SqlException sqlEx:
                status = StatusCodes.Status500InternalServerError;
                problem = new ProblemDetails
                {
                    Title = "Database error",
                    Status = status,
                    Detail = ShortenSqlMessage(sqlEx.Message)
                };
                break;

            case SocketException sockEx:
                status = StatusCodes.Status500InternalServerError;
                problem = new ProblemDetails
                {
                    Title = "Network error",
                    Status = status,
                    Detail = "Nie można nawiązać połączenia z bazą danych."
                };
                break;

            case Win32Exception win32Ex:
                status = StatusCodes.Status500InternalServerError;
                problem = new ProblemDetails
                {
                    Title = "System error",
                    Status = status,
                    Detail = "Nie można nawiązać połączenia z bazą danych."
                };
                break;

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
                    var innermostEx = GetInnermostException(dbEx);

                    if (innermostEx is SqlException innerSqlEx)
                    {
                        if (innerSqlEx.Number == 2627 || innerSqlEx.Number == 2601)
                        {
                            var indexName = ExtractIndexNameFromSqlMessage(innerSqlEx.Message);
                            var indexMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                            {
                                ["IX_Clients_Nip_UserId"] = "Istnieje już kontrahent o tym nr NIP"
                            };

                            var detailMsg = (!string.IsNullOrEmpty(indexName) && indexMap.TryGetValue(indexName, out var userMsg))
                                ? userMsg
                                : ShortenSqlMessage(innerSqlEx.Message);

                            status = StatusCodes.Status409Conflict;
                            problem = new ProblemDetails
                            {
                                Title = "Conflict",
                                Status = status,
                                Detail = detailMsg
                            };
                            break;
                        }

                        status = StatusCodes.Status500InternalServerError;
                        problem = new ProblemDetails
                        {
                            Title = "Database error",
                            Status = status,
                            Detail = ShortenSqlMessage(innerSqlEx.Message)
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
                {
                    var innermostDefault = GetInnermostException(exception);
                    if (innermostDefault is SqlException sqlEx2)
                    {
                        status = StatusCodes.Status500InternalServerError;
                        problem = new ProblemDetails
                        {
                            Title = "Database error",
                            Status = status,
                            Detail = ShortenSqlMessage(sqlEx2.Message)
                        };
                    }
                    else
                    {
                        status = StatusCodes.Status500InternalServerError;
                        problem = new ProblemDetails
                        {
                            Title = "Internal Server Error",
                            Status = status,
                            Detail = exception.Message
                        };
                    }
                }
                break;
        }

        if (!string.IsNullOrEmpty(context.TraceIdentifier))
        {
            problem.Extensions["traceId"] = context.TraceIdentifier;
        }

        context.Response.Clear();
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        try
        {
            _logger.LogInformation("Writing ProblemDetails response for {Path} with status {Status}. TraceId={TraceId}", context.Request?.Path, status, context.TraceIdentifier);
            var json = JsonSerializer.Serialize(problem, jsonOptions);
            await context.Response.WriteAsync(json).ConfigureAwait(false);
        }
        catch (Exception writeEx)
        {
            _logger.LogError(writeEx, "Failed to write ProblemDetails JSON for {Path}. TraceId={TraceId}", context.Request?.Path, context.TraceIdentifier);

            try
            {
                var fallback = $"{{\"title\":\"{EscapeForJson(problem.Title)}\",\"status\":{status},\"detail\":\"{EscapeForJson(problem.Detail)}\",\"traceId\":\"{EscapeForJson(context.TraceIdentifier)}\"}}";
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

    private static bool IsConnectionIssue(Exception? ex)
    {
        if (ex == null) return false;

        if (ex is SqlException) return true;
        if (ex is SocketException) return true;
        if (ex is Win32Exception) return true;

        var msg = ex.Message ?? string.Empty;
        var lower = msg.ToLowerInvariant();

        if (lower.Contains("a network-related or instance-specific error")
            || lower.Contains("nie można nawiązać połączenia")
            || lower.Contains("actively refused")
            || lower.Contains("tcp provider")
            || lower.Contains("cannot open database")
            || lower.Contains("login failed")
            || lower.Contains("microsoft.data.sqlclient")
            || lower.Contains("sqlexception"))
        {
            return true;
        }

        return false;
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

    private static string ShortenSqlMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) return "Wystąpił błąd z bazą danych.";

        var lower = message.ToLowerInvariant();

        if (lower.Contains("a network-related or instance-specific error")
            || lower.Contains("nie można nawiązać połączenia")
            || lower.Contains("actively refused")
            || lower.Contains("tcp provider")
            || lower.Contains("sqlexception")
            || lower.Contains("microsoft.data.sqlclient")
            || lower.Contains("provider: tcp provider"))
        {
            return "Nie można nawiązać połączenia z bazą danych.";
        }

        return "Wystąpił błąd z bazą danych.";
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}