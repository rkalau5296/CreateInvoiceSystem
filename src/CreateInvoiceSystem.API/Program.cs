using CreateInvoiceSystem.API.DI;
using CreateInvoiceSystem.API.Middleware;
using CreateInvoiceSystem.API.RestServices;
using CreateInvoiceSystem.API.TransactionBehavior;
using CreateInvoiceSystem.API.ValidationBehavior;
using CreateInvoiceSystem.Csv.Controllers;
using CreateInvoiceSystem.Csv.DI;
using CreateInvoiceSystem.Identity.DI;
using CreateInvoiceSystem.Mail.DI;
using CreateInvoiceSystem.Modules.Invoices.Application.Services;
using CreateInvoiceSystem.Modules.Invoices.BackgroundTasks;
using CreateInvoiceSystem.Modules.Nbp.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.DI;
using CreateInvoiceSystem.Modules.Nbp.Interfaces;
using CreateInvoiceSystem.Modules.Users.DI;
using CreateInvoiceSystem.Pdf.Extensions;
using CreateInvoiceSystem.Persistence;
using CreateInvoiceSystem.Persistence.DI;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog.Web;
using System.Globalization;
using System.Net.Http.Headers;
using System.Threading.Channels;

var cultureInfo = new CultureInfo("pl-PL");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
#endif

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = false;
}).AddJsonOptions(_ => { })
  .AddMvcOptions(mvcOptions =>
      mvcOptions.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true)
  .AddApplicationPart(typeof(ExportController).Assembly);

builder.Services.AddSwaggerModule();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddIdentityModule();
builder.Services.AddAuthModule(builder.Configuration);
builder.Services.AddApiAdapters();
builder.Services.AddApiRepositories();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
builder.Services.AddApplication();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
    options.AddPolicy("AllowConfiguredOrigins", policy =>
    {
        var frontendUrl = builder.Configuration["FrontendUrl"]?.TrimEnd('/');

        policy.WithOrigins(frontendUrl ?? string.Empty)
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddCsvModule();
builder.Services.AddPdfModule();
builder.Services.AddNbpModule(builder.Configuration);
builder.Services.AddHttpClient<INbpApiRestService, NbpApiRestService>((serviceProvider, client) =>
{
    var options =
        serviceProvider.GetRequiredService<IOptions<NbpApiOptions>>();

    var baseUrl = options.Value.BaseUrl
        ?? throw new InvalidOperationException(
            "NbpApi:BaseUrl is not configured.");

    client.BaseAddress = new Uri(baseUrl);

    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddMailModule();
builder.Services.AddUserModule();
builder.Logging.ClearProviders();
builder.Host.UseNLog();
builder.Services.AddSingleton(Channel.CreateUnbounded<EmailTask>());
builder.Services.AddSingleton(sp => sp.GetRequiredService<Channel<EmailTask>>().Writer);
builder.Services.AddSingleton(sp => sp.GetRequiredService<Channel<EmailTask>>().Reader);
builder.Services.AddHostedService<EmailSendingService>();

var app = builder.Build();

app.UseExceptionHandling();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(app.Environment.IsDevelopment() ? "AllowAll" : "AllowConfiguredOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.UseSessionActivityTracking();
app.UseSwaggerModule();
app.MapControllers();
app.Run();

public partial class Program { }