using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClients;
using CreateInvoiceSystem.Modules.Clients.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.GetInvoices;
using CreateInvoiceSystem.Modules.Invoices.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Nbp.Domain.Application.RequestResponse.ActualRates;
using CreateInvoiceSystem.Modules.Products.Domain.Application.RequestsResponses.GetProducts;
using CreateInvoiceSystem.Modules.Products.Domain.Application.Validators;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Commands;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.GetUsers;
using CreateInvoiceSystem.Modules.Users.Domain.Application.Validators;
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
            typeof(ActivateUserCommand).Assembly
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
