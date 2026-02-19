using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Executors;
using CreateInvoiceSystem.API.Adapters.CsvDataAdapter;
using CreateInvoiceSystem.API.Adapters.InvoiceEmailAdapter;
using CreateInvoiceSystem.API.Adapters.PdfAdapter;
using CreateInvoiceSystem.API.Adapters.UserAuthAdapter;
using CreateInvoiceSystem.API.Adapters.UserEmailAdapter;
using CreateInvoiceSystem.API.Adapters.UserTokenAdapter;
using CreateInvoiceSystem.API.Middleware;
using CreateInvoiceSystem.API.Repositories.ClientRepository;
using CreateInvoiceSystem.API.Repositories.InvoiceRepository;
using CreateInvoiceSystem.API.Repositories.ProductRepository;
using CreateInvoiceSystem.API.Repositories.UserRepository;
using CreateInvoiceSystem.API.RestServices;
using CreateInvoiceSystem.API.ValidationBehavior;
using CreateInvoiceSystem.Csv.Controllers;
using CreateInvoiceSystem.Csv.Interfaces;
using CreateInvoiceSystem.Csv.Services;
using CreateInvoiceSystem.Identity.Interfaces;
using CreateInvoiceSystem.Identity.Services;
using CreateInvoiceSystem.Mail;
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
using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.GetUsers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Services;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Persistence.Entities;
using CreateInvoiceSystem.Modules.Users.Persistence.Persistence;
using CreateInvoiceSystem.Pdf.Extensions;
using CreateInvoiceSystem.Persistence;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Web;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#if DEBUG
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);
#endif

// Controllers + JSON + Accept negotiation
builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable = false;
}).AddJsonOptions(_ => { })
.AddApplicationPart(typeof(ExportController).Assembly);

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CreateInvoiceSystem API",
        Version = "v1"
    });
    
    c.CustomSchemaIds(t => t.FullName!.Replace('+', '.'));

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Wklej token JWT: Bearer {twój_token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// MediatR 
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
    typeof(GetClientsRequest).Assembly,
    typeof(GetProductsRequest).Assembly,
    typeof(GetUsersRequest).Assembly,
    typeof(GetActualCurrencyRatesRequest).Assembly,
    typeof(GetInvoicesRequest).Assembly,
    typeof(ActivateUserCommand).Assembly
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

// NBP options 
builder.Services.Configure<NbpApiOptions>(builder.Configuration.GetSection("NbpApi"));
builder.Services.AddScoped<INbpApiRestService, NbpApiRestService>();

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

// DbContext
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
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IJwtProvider, JwtProvider>();
builder.Services.AddScoped<IUserTokenService, UserTokenAdapter>();
builder.Services.AddTransient<IEmailService, SmtpEmailService>();
builder.Services.AddTransient<IUserEmailSender, UserEmailAdapter>();
builder.Services.AddTransient<IInvoiceEmailSender, InvoiceEmailAdapter>();
builder.Services.AddScoped<IUserAuthService, UserAuthServiceAdapter>();
builder.Services.AddIdentity<UserEntity, IdentityRole<int>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<CreateInvoiceSystemDbContext>()
.AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ClockSkew = TimeSpan.Zero
    };
});

// Logging
builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Host.UseNLog();

//csv
builder.Services.AddScoped<ICsvExportService, CsvExportService>();
builder.Services.AddScoped<IExportDataProvider, InvoiceExportDataProvider>();

//pdf
builder.Services.AddPdfModule();
builder.Services.AddScoped<IInvoiceExportService, InvoiceToPdfAdapter>();

// BackgroundService
builder.Services.AddHostedService<UserCleanupService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policy =>
    {
        policy.WithOrigins("https://localhost:7022", "http://localhost:5004")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowBlazor");
app.UseMiddleware<ValidationExceptionMiddleware>();
app.UseAuthentication(); 
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CreateInvoiceSystem API v1");
    c.RoutePrefix = string.Empty;
});
app.MapControllers();
app.Run();