using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.Addresses.Application.RequestsResponses.CreateAddress;
using CreateInvoiceSystem.Addresses.Application.RequestsResponses.GetAddresses;
using CreateInvoiceSystem.Addresses.Application.ValidationBehavior;
using CreateInvoiceSystem.Addresses.Application.Validators;
using CreateInvoiceSystem.API.Middleware;
using CreateInvoiceSystem.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetAddressesRequest).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(CreateAddressRequestValidator).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<CreateInvoiceSystemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IDbContext, CreateInvoiceSystemDbContext>();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRouting();
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


app.MapGet("/test", () =>
{
    throw new FluentValidation.ValidationException(new[]
    {
        new FluentValidation.Results.ValidationFailure("TestField", "Test message")
    });
});

app.MapPost("/addresses", HandleCreateAddress)
    .Produces<CreateAddressResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status400BadRequest);

app.MapControllers();

app.Run();

static async Task<IResult> HandleCreateAddress(CreateAddressRequest request, IMediator mediator)
{
    var result = await mediator.Send(request);
    return Results.Ok(result);
}
