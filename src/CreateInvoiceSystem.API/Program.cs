using CreateInvoiceSystem.API.Adapters.InvoiceEmailAdapter;
using CreateInvoiceSystem.API.Adapters.KsefInvoicePdfAdapter;
using CreateInvoiceSystem.API.DI;
using CreateInvoiceSystem.API.InvoiceCreatedBehavior;
using CreateInvoiceSystem.API.Middleware;
using CreateInvoiceSystem.API.RestServices;
using CreateInvoiceSystem.API.TransactionBehavior;
using CreateInvoiceSystem.API.ValidationBehavior;
using CreateInvoiceSystem.Csv.Controllers;
using CreateInvoiceSystem.Csv.DI;
using CreateInvoiceSystem.Identity.DI;
using CreateInvoiceSystem.Mail.DI;
using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.CloneInvoice;
using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.CreateInvoice;
using CreateInvoiceSystem.Modules.Ksef.Application.Options;
using CreateInvoiceSystem.Modules.Ksef.DI;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using CreateInvoiceSystem.Modules.Nbp.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.DI;
using CreateInvoiceSystem.Modules.Nbp.Interfaces;
using CreateInvoiceSystem.Modules.Users.DI;
using CreateInvoiceSystem.Pdf;
using CreateInvoiceSystem.Pdf.Extensions;
using CreateInvoiceSystem.Persistence.DI;
using CreateInvoiceSystem.Persistence;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NLog.Web;
using System.Globalization;
using System.Net.Http.Headers;
using System.Security.Authentication;

var cultureInfo = new CultureInfo("pl-PL");
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
#endif

builder.Services.AddKsefDataProtection(builder.Configuration, builder.Environment);
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
builder.Services.AddTransient<IPipelineBehavior<CreateInvoiceRequest, CreateInvoiceResponse>, InvoiceCreatedBehavior>();
builder.Services.AddTransient<IPipelineBehavior<CloneInvoiceRequest, CloneInvoiceResponse>, CloneInvoiceBehavior>();
builder.Services.AddApplication();

var rabbitMqHost = builder.Configuration["RabbitMq:Host"] ?? "localhost";
var rabbitMqPort = builder.Configuration.GetValue<ushort>("RabbitMq:Port", 5672);
var rabbitMqVirtualHost = builder.Configuration["RabbitMq:VirtualHost"] ?? "/";
var rabbitMqUsername = builder.Configuration["RabbitMq:Username"] ?? "guest";
var rabbitMqPassword = builder.Configuration["RabbitMq:Password"] ?? "guest";
var rabbitMqUseSsl = builder.Configuration.GetValue<bool>("RabbitMq:UseSsl");

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<InvoiceCreatedEmailConsumer>();
    x.AddEntityFrameworkOutbox<CreateInvoiceSystemDbContext>(o =>
    {
        o.UseSqlServer();
        o.UseBusOutbox();
    });
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqHost, rabbitMqPort, rabbitMqVirtualHost, host =>
        {
            host.Username(rabbitMqUsername);
            host.Password(rabbitMqPassword);
            if (rabbitMqUseSsl)
                host.UseSsl(ssl => ssl.Protocol = SslProtocols.Tls12);
        });
        cfg.ReceiveEndpoint("invoice-created-email", e =>
        {
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
            e.ConfigureConsumer<InvoiceCreatedEmailConsumer>(context);
        });
    });

});

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
builder.Services.AddScoped<KsefInvoicePdfGenerator>();
builder.Services.AddScoped<IKsefInvoicePdfGenerator, KsefInvoicePdfAdapter>();
builder.Services.AddNbpModule(builder.Configuration);
builder.Services.AddKsefModule(builder.Configuration);
builder.Services.AddHttpClient<IKsefInvoiceBytesClient, KsefInvoiceBytesClient>((services, client) =>
{
    var options = services.GetRequiredService<IOptions<KsefApiOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});
builder.Services.AddScoped<IKsefApiClient, KsefApiClient>();
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