using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Addresses.Application.RequestsResponses.GetAddresses;
using CreateInvoiceSystem.Addresses.Application.ValidationBehavior;
using CreateInvoiceSystem.Addresses.Application.Validators;
using CreateInvoiceSystem.API.Filters;
using CreateInvoiceSystem.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using CreateInvoiceSystem.API.Middleware;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAddressesRequest).Assembly));
builder.Services.AddValidatorsFromAssemblyContaining<CreateAddressRequestValidator>();
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
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


builder.Services.AddTransient<ICommandExecutor, CommandExecutor>();
builder.Services.AddTransient<IQueryExecutor, QueryExecutor>();
builder.Services.AddControllers();
//builder.Services.AddControllers(options =>
//{
//    options.Filters.Add<ValidationExceptionFilter>();
//});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<CreateInvoiceSystemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IDbContext, CreateInvoiceSystemDbContext>();

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
