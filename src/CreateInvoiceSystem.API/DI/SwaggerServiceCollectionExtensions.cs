using Microsoft.OpenApi.Models;

namespace CreateInvoiceSystem.API.DI;

public static class SwaggerServiceCollectionExtensions
{
    public static IServiceCollection AddSwaggerModule(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
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

        return services;
    }
    
    public static WebApplication UseSwaggerModule(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "CreateInvoiceSystem API v1");
                c.RoutePrefix = string.Empty;
            });
        }

        return app;
    }
}
