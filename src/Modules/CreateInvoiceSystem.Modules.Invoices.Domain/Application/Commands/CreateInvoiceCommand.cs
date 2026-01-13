using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Commands;
public class CreateInvoiceCommand : CommandBase<CreateInvoiceDto, InvoiceDto, IInvoiceRepository>
{
    public override async Task<InvoiceDto> Execute(IInvoiceRepository _invoiceRepository, CancellationToken cancellationToken = default)
    {
        ValidateInvoiceParametr(Parametr);               

        Client client = Parametr.ClientId is null
            ? await GetOrCreateClientAsync(Parametr, _invoiceRepository, cancellationToken)
            : await GetClientByIdAsync(Parametr.ClientId.Value, _invoiceRepository, cancellationToken);

        Invoice entity = Parametr.ClientId is null
            ? InvoiceMappers.ToInvoiceWithNewClient(Parametr, client)
            : InvoiceMappers.ToInvoiceWithExistingClient(Parametr, client);

        await AddProductsToInvoicePositionsAsync(Parametr, entity, _invoiceRepository, cancellationToken);

        var createdInvoice = await _invoiceRepository.AddInvoiceAsync(entity, cancellationToken);
        await _invoiceRepository.SaveChangesAsync(cancellationToken);

        var persisted = await _invoiceRepository.GetInvoiceByIdAsync(
            createdInvoice.InvoiceId,            
            cancellationToken);

        bool added = persisted is not null
            && persisted.Client is not null
            && persisted.InvoicePositions is not null;

        //if (persisted.UserId != null && !string.IsNullOrEmpty(persisted.User.Email))
        //{

        //    await _mailService.SendInvoiceCreatedEmailAsync(
        //        user.Email,
        //        Parametr.Title,
        //        Parametr.TotalAmount,
        //        cancellationToken);
        //}

        return added 
            ? entity.ToDto()
            : throw new InvalidOperationException("User was saved but could not be reloaded.");      
    }

    private static void ValidateInvoiceParametr(CreateInvoiceDto parametr)
    {
        ArgumentNullException.ThrowIfNull(parametr);

        if (parametr.InvoicePositions is null || parametr.InvoicePositions.Count == 0)
            throw new InvalidOperationException("Invoice must contain at least one position.");

        if (parametr.ClientId is null && (parametr.Client is null || IsClientDtoEmpty(parametr.Client)))
            throw new InvalidOperationException("Invoice must contain clientId or Client details.");       

        if (parametr.InvoicePositions.Any(ip => ip.Product is null && ip.ProductId is null))
            throw new InvalidOperationException("InvoicePosition must contain Product or ProductId details.");
    }

    private static bool IsClientDtoEmpty(CreateClientDto client)
    {
        if (client == null)
            return true;

        return string.IsNullOrEmpty(client.Name)
            && string.IsNullOrEmpty(client.Nip)
            && (
                client.Address == null ||
                
                    string.IsNullOrEmpty(client.Address.Street) &&
                    string.IsNullOrEmpty(client.Address.Number) &&
                    string.IsNullOrEmpty(client.Address.City) &&
                    string.IsNullOrEmpty(client.Address.PostalCode) &&
                    string.IsNullOrEmpty(client.Address.Country)
                
            );
    }

    private static async Task<Client> GetOrCreateClientAsync(CreateInvoiceDto param, IInvoiceRepository _invoiceRepository, CancellationToken cancellationToken)
    {       
        var client = await _invoiceRepository.GetClientAsync(
            param.Client.Name,
            param.Client.Address.Street,
            param.Client.Address.Number,
            param.Client.Address.City,
            param.Client.Address.PostalCode,
            param.Client.Address.Country,
            param.UserId,
            cancellationToken);

        if (client is not null)
            return client;

        var newClient = InvoiceMappers.ToEntity(param.Client);
        newClient.UserId = param.UserId;
        
        await _invoiceRepository.AddClientAsync(newClient, cancellationToken);
        return newClient;
    }

    private static async Task<Client> GetClientByIdAsync(int clientId, IInvoiceRepository _invoiceRepository, CancellationToken cancellationToken)
    {       
        return await _invoiceRepository.GetClientByIdAsync(clientId, cancellationToken) ?? throw new InvalidOperationException($"Client with ID {clientId} not found.");
    }

    private static async Task AddProductsToInvoicePositionsAsync(CreateInvoiceDto param, Invoice entity, IInvoiceRepository _invoiceRepository, CancellationToken cancellationToken)
    {
        foreach (var position in param.InvoicePositions)
        {
            var product = position.ProductId is null
                ? await GetOrCreateProductAsync(position, param.UserId, _invoiceRepository, cancellationToken)
                : await GetProductByIdAsync(position.ProductId.Value, _invoiceRepository, cancellationToken);

            var invoicePosition = new InvoicePosition
            {
                Quantity = position.Quantity,
                Product = product,
                ProductId = product.ProductId,
                ProductName = product.Name,
                ProductDescription = product.Description,
                ProductValue = product.Value
            };
            entity.InvoicePositions.Add(invoicePosition);
        }
    }

    private static async Task<Product> GetOrCreateProductAsync(InvoicePositionDto position, int userId, IInvoiceRepository _invoiceRepository, CancellationToken cancellationToken)
    {
        var existing = await _invoiceRepository.GetProductAsync(
            position.Product.Name,
            position.Product.Description,
            position.Product.Value,
            userId,
            cancellationToken);
        
        if (existing is not null) return existing;

        var newProduct = new Product
        {
            UserId = userId,
            Name = position.Product.Name,
            Description = position.Product.Description,
            Value = position.Product.Value
        };
        await _invoiceRepository.AddProductAsync(newProduct, cancellationToken);        
        return newProduct;
    }
    
    private static async Task<Product> GetProductByIdAsync(int productId, IInvoiceRepository _invoiceRepository, CancellationToken cancellationToken)
    {        
        return await _invoiceRepository.GetProductByIdAsync(productId, cancellationToken) ?? throw new InvalidOperationException($"Product with ID {productId} not found.");

    }    
}