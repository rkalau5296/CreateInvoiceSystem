using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.API.Middleware;
using CreateInvoiceSystem.API.Repositories.ClientRepository;
using CreateInvoiceSystem.API.Repositories.InvoiceRepository;
using CreateInvoiceSystem.API.Repositories.ProductRepository;
using CreateInvoiceSystem.API.Repositories.UserRepository;
using CreateInvoiceSystem.API.RestServices;
using CreateInvoiceSystem.API.ValidationBehavior;
using CreateInvoiceSystem.Modules.Addresses.Persistence.Persistence;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClients;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Clients.Persistence.Persistence;
using CreateInvoiceSystem.Modules.InvoicePositions.Persistence.Persistence;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoices;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Persistence.Persistence;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRates;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProducts;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Persistence.Persistence;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.GetUsers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Persistence.Persistence;
using CreateInvoiceSystem.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using NLog.Web;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
#endif

// Controllers + JSON + Accept negotiation
builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = false;
}).AddJsonOptions(_ => { });

// Swagger — pojedyncza rejestracja + unikalne ID schematów
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CreateInvoiceSystem API",
        Version = "v1"
    });

    // Kluczowe: unikaj kolizji nazw schematów (te same nazwy DTO w ró¿nych namespace'ach)
    c.CustomSchemaIds(t => t.FullName!.Replace('+', '.'));
});

// MediatR — jedna, skonsolidowana rejestracja
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(GetClientsRequest).Assembly,
    typeof(GetProductsRequest).Assembly,
    typeof(GetUsersRequest).Assembly,
    typeof(GetActualCurrencyRatesRequest).Assembly,
    typeof(GetInvoicesRequest).Assembly
));

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateClientRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateClientRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateProductRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateInvoiceRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateInvoiceRequestValidator>();

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Repositories (Scoped)
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// NBP options + typed HttpClient
builder.Services.Configure<NbpApiOptions>(builder.Configuration.GetSection("NbpApi"));
builder.Services.AddHttpClient<INbpApiRestService, NbpApiRestService>((sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<NbpApiOptions>>().Value;
    client.BaseAddress = new Uri(opts.BaseUrl);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

// API behavior
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Executors
builder.Services.AddTransient<ICommandExecutor, CommandExecutor>();
builder.Services.AddTransient<IQueryExecutor, QueryExecutor>();

// DbContext: rejestruj KONKRETN¥ klasê, a interfejsy mapuj do tej samej instancji
builder.Services.AddDbContext<CreateInvoiceSystemDbContext>(db =>
    db.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure()));

builder.Services.AddScoped<IAddressDbContext>(sp => sp.GetRequiredService<CreateInvoiceSystemDbContext>());
builder.Services.AddScoped<IClientDbContext>(sp => sp.GetRequiredService<CreateInvoiceSystemDbContext>());
builder.Services.AddScoped<IProductDbContext>(sp => sp.GetRequiredService<CreateInvoiceSystemDbContext>());
builder.Services.AddScoped<IInvoicePosistionDbContext>(sp => sp.GetRequiredService<CreateInvoiceSystemDbContext>());
builder.Services.AddScoped<IInvoiceDbContext>(sp => sp.GetRequiredService<CreateInvoiceSystemDbContext>());
builder.Services.AddScoped<IUserDbContext>(sp => sp.GetRequiredService<CreateInvoiceSystemDbContext>());
builder.Services.AddScoped<IDbContext>(sp => sp.GetRequiredService<CreateInvoiceSystemDbContext>());


// Logging
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Host.UseNLog();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAll");
app.UseMiddleware<ValidationExceptionMiddleware>();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CreateInvoiceSystem API v1");
    c.RoutePrefix = "swagger";
});

app.MapControllers();
app.Run();