using CreateInvoiceSystem.Abstractions.DI;
using CreateInvoiceSystem.API.DI;
using CreateInvoiceSystem.API.Middleware;
using CreateInvoiceSystem.API.RestServices;
using CreateInvoiceSystem.API.TransactionBehavior;
using CreateInvoiceSystem.API.ValidationBehavior;
using CreateInvoiceSystem.Csv.Controllers;
using CreateInvoiceSystem.Csv.DI;
using CreateInvoiceSystem.Identity.DI;
using CreateInvoiceSystem.Mail.DI;
using CreateInvoiceSystem.Modules.Nbp.Domain.DI;
using CreateInvoiceSystem.Modules.Nbp.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.DI;
using CreateInvoiceSystem.Pdf.Extensions;
using CreateInvoiceSystem.Persistence;
using CreateInvoiceSystem.Persistence.DI;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
#endif

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = false;
}).AddJsonOptions(_ => { })
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
});

builder.Services.AddAbstractionsModule();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCsvModule();
builder.Services.AddPdfModule();
builder.Services.AddNbpModule(builder.Configuration);
builder.Services.AddScoped<INbpApiRestService, NbpApiRestService>();
builder.Services.AddMailModule();
builder.Services.AddUserModule();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins("https://localhost:7022", "http://localhost:5004")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Host.UseNLog();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CreateInvoiceSystemDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "B³¹d podczas automatycznej migracji bazy danych.");
    }
}

app.UseExceptionHandling();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowBlazor");
app.UseAuthentication();
app.UseAuthorization();
app.UseSwaggerModule();
app.MapControllers();
app.Run();

public partial class Program { }