using CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.GetClients;
using CreateInvoiceSystem.Modules.Clients.Application.Validators;
using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.GetInvoices;
using CreateInvoiceSystem.Modules.Invoices.Application.Validators;
using CreateInvoiceSystem.Modules.Nbp.Application.RequestResponse.ActualRates;
using CreateInvoiceSystem.Modules.Products.Application.RequestsResponses.GetProducts;
using CreateInvoiceSystem.Modules.Products.Application.Validators;
using CreateInvoiceSystem.Modules.Users.Application.Handlers;
using CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.GetUsers;
using CreateInvoiceSystem.Modules.Users.Application.Validators;
using FluentValidation;

namespace CreateInvoiceSystem.API.DI;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
            typeof(GetClientsRequest).Assembly,
            typeof(GetProductsRequest).Assembly,
            typeof(GetUsersRequest).Assembly,
            typeof(GetActualCurrencyRatesRequest).Assembly,
            typeof(GetInvoicesRequest).Assembly,
            typeof(ActivateUserHandler).Assembly
        ));

        services.AddValidatorsFromAssemblyContaining<CreateClientRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateClientRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateProductRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateUserRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateUserRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<CreateInvoiceRequestValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateInvoiceRequestValidator>();
        
        return services;
    }
}
