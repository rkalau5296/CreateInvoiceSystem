using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Addresses.Application.RequestsResponses.CreateAddress;
using CreateInvoiceSystem.Addresses.Application.RequestsResponses.GetAddresses;
using CreateInvoiceSystem.Persistence;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddTransient<ICommandExecutor, CommandExecutor>();
builder.Services.AddTransient<IQueryExecutor, QueryExecutor>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAddressesRequest).Assembly));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<CreateInvoiceSystemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IDbContext, CreateInvoiceSystemDbContext>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{    
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CreateInvoiceSystem API v1");
        c.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();