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

        if (context.Response.HasStarted) return;

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
            await WriteResponseAsync(context, problem, status, jsonOptions);
            return;
        }

        switch (exception)
        {
            case DbUpdateException dbEx:
                var innermostEx = GetInnermostException(dbEx);
                if (innermostEx is SqlException innerSqlEx && (innerSqlEx.Number == 2627 || innerSqlEx.Number == 2601))
                {
                    status = StatusCodes.Status409Conflict;
                    var indexName = ExtractIndexNameFromSqlMessage(innerSqlEx.Message);
                    var indexMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["IX_Clients_Nip_UserId"] = "Istnieje już kontrahent o tym nr NIP"
                    };

                    problem = new ProblemDetails
                    {
                        Title = "Conflict",
                        Status = status,
                        Detail = (indexName != null && indexMap.TryGetValue(indexName, out var msg)) ? msg : "Istnieje już w bazie klient z tymi danymi."
                    };
                }
                else
                {
                    status = StatusCodes.Status500InternalServerError;
                    problem = new ProblemDetails { Title = "Database error", Status = status, Detail = ShortenSqlMessage(innermostEx.Message) };
                }
                break;

            case ArgumentException:
                status = StatusCodes.Status400BadRequest;
                problem = new ProblemDetails { Title = "Bad Request", Status = status, Detail = exception.Message };
                break;

            case InvalidOperationException:
                if (exception.Message.Contains("Sequence contains no elements") || exception.Message.Contains("not found"))
                {
                    status = StatusCodes.Status404NotFound;
                    problem = new ProblemDetails { Title = "Not Found", Status = status, Detail = "Szukany zasób nie istnieje." };
                }
                else
                {
                    status = StatusCodes.Status400BadRequest;
                    problem = new ProblemDetails { Title = "Invalid Operation", Status = status, Detail = exception.Message };
                }
                break;

            case KeyNotFoundException:
                status = StatusCodes.Status404NotFound;
                problem = new ProblemDetails { Title = "Not Found", Status = status, Detail = exception.Message };
                break;

            case FluentValidation.ValidationException fvEx:
                status = StatusCodes.Status400BadRequest;
                problem = new ValidationProblemDetails(fvEx.Errors.GroupBy(e => e.PropertyName).ToDictionary(g => g.Key ?? string.Empty, g => g.Select(e => e.ErrorMessage).ToArray()))
                {
                    Title = "Validation failed",
                    Status = status,
                    Detail = "Wystąpiły błędy walidacji."
                };
                break;

            case UnauthorizedAccessException:
                status = StatusCodes.Status401Unauthorized;
                problem = new ProblemDetails { Title = "Unauthorized", Status = status, Detail = exception.Message };
                break;

            default:
                status = StatusCodes.Status500InternalServerError;
                problem = new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Status = status,
                    Detail = _env.IsDevelopment() ? exception.Message : "Wystąpił nieoczekiwany błąd serwera."
                };
                break;
        }

        await WriteResponseAsync(context, problem, status, jsonOptions);
    }

    private async Task WriteResponseAsync(HttpContext context, ProblemDetails problem, int status, JsonSerializerOptions options)
    {
        problem.Extensions["traceId"] = context.TraceIdentifier;
        context.Response.Clear();
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, options)).ConfigureAwait(false);
    }

    private static bool IsConnectionIssue(Exception? ex)
    {
        if (ex is SqlException sqlEx)
        {
            if (sqlEx.Number == 2627 || sqlEx.Number == 2601) return false;
            int[] connErrors = { -1, -2, 2, 53, 4060, 10054, 10060, 10061, 18456 };
            return connErrors.Contains(sqlEx.Number);
        }
        return ex is SocketException || ex is Win32Exception;
    }

    private static string? ExtractIndexNameFromSqlMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message)) return null;
        var m = Regex.Match(message, @"unique (?:index|constraint)\s+'(?<idx>[^']+)'", RegexOptions.IgnoreCase);
        return m.Success ? m.Groups["idx"].Value : null;
    }

    private static string ShortenSqlMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message)) return "Wystąpił błąd bazy danych.";
        return message.ToLower().Contains("unique") ? "Istnieje już taki rekord." : "Wystąpił błąd z bazą danych.";
    }

    private static Exception GetInnermostException(Exception ex)
    {
        while (ex.InnerException != null) ex = ex.InnerException;
        return ex;
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}