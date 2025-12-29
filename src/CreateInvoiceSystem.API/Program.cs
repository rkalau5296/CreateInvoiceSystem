using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.API.Middleware;
using CreateInvoiceSystem.API.Repositories.ClientRepository;
using CreateInvoiceSystem.API.Repositories.InvoiceRepository;
using CreateInvoiceSystem.API.Repositories.ProductRepository;
using CreateInvoiceSystem.API.Repositories.UserRepository;
using CreateInvoiceSystem.API.RestServices;
using CreateInvoiceSystem.API.ValidationBehavior;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClients;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Clients.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoices;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.Options;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRates;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProducts;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Products.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.GetUsers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NLog.Web;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
#endif

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetClientsRequest).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetProductsRequest).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetUsersRequest).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetActualCurrencyRatesRequest).Assembly));
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetInvoicesRequest).Assembly));
builder.Services.Configure<NbpApiOptions>(builder.Configuration.GetSection("NbpApi"));
builder.Services.AddValidatorsFromAssemblyContaining<CreateClientRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateClientRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateProductRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateInvoiceRequestValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateInvoiceRequestValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddHttpClient<INbpApiRestService, NbpApiRestService>((sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<NbpApiOptions>>().Value;
    client.BaseAddress = new Uri(opts.BaseUrl);
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
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
});

builder.Services.AddTransient<IClientRepository, ClientRepository>();
builder.Services.AddTransient<ICommandExecutor, CommandExecutor>();
builder.Services.AddTransient<IQueryExecutor, QueryExecutor>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ICreateInvoiceSystemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), options => options.EnableRetryOnFailure()));
builder.Services.AddScoped<IDbContext, ICreateInvoiceSystemDbContext>();
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Host.UseNLog();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseMiddleware<ValidationExceptionMiddleware>();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CreateInvoiceSystem API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.MapControllers();
app.Run();
