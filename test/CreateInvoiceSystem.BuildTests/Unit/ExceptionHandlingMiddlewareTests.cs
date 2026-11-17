using System.Net.Sockets;
using System.Text.Json;
using CreateInvoiceSystem.API.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CreateInvoiceSystem.BuildTests.Unit;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task Http_connection_failure_is_not_reported_as_a_database_error()
    {
        var exception = new HttpRequestException("Unknown host", new SocketException((int)SocketError.HostNotFound));
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw exception,
            NullLogger<ExceptionHandlingMiddleware>.Instance,
            Mock.Of<IHostEnvironment>());
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
        context.Response.Body.Position = 0;
        var problem = await JsonSerializer.DeserializeAsync<ProblemDetails>(
            context.Response.Body, new JsonSerializerOptions(JsonSerializerDefaults.Web), TestContext.Current.CancellationToken);
        Assert.NotNull(problem);
        Assert.Contains("usługą zewnętrzną", problem.Detail);
        Assert.DoesNotContain("bazą danych", problem.Detail);
    }
}
