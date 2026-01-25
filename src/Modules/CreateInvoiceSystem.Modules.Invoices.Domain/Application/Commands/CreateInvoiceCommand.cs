using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Commands;
public class CreateInvoiceCommand : CommandBase<CreateInvoiceDto, InvoiceDto, IInvoiceRepository>
{
    private readonly IInvoiceEmailSender _emailSender;
    
    public CreateInvoiceCommand(CreateInvoiceDto dto, IInvoiceEmailSender emailSender)
    {
        this.Parametr = dto;
        _emailSender = emailSender;
    }

    public override async Task<InvoiceDto> Execute(IInvoiceRepository _invoiceRepository, CancellationToken cancellationToken = default)
    {
        ValidateInvoiceParametr(Parametr);               

        Client client = Parametr.ClientId is null
            ? await GetOrCreateClientAsync(Parametr, _invoiceRepository, cancellationToken)
            : await GetClientByIdAsync(Parametr.ClientId.Value, _invoiceRepository, cancellationToken);

        User user = await _invoiceRepository.GetUserByIdAsync(Parametr.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"User with ID {Parametr.UserId} not found.");

        Invoice entity = Parametr.ClientId is null
            ? InvoiceMappers.ToInvoiceWithNewClient(Parametr, client, user)
            : InvoiceMappers.ToInvoiceWithExistingClient(Parametr, client, user);        

        await AddProductsToInvoicePositionsAsync(Parametr, entity, _invoiceRepository, cancellationToken);

        entity.RecalculateTotals();

        entity.Title = await GenerateInvoiceNumberAsync(Parametr.UserId, _invoiceRepository, cancellationToken);

        var createdInvoice = await _invoiceRepository.AddInvoiceAsync(entity, cancellationToken);
        await _invoiceRepository.SaveChangesAsync(cancellationToken);

        var userEmail = await _invoiceRepository.GetUserEmailByIdAsync(Parametr.UserId, cancellationToken);

        if (!string.IsNullOrEmpty(userEmail))
        {
            await _emailSender.SendInvoiceCreatedEmailAsync(userEmail, entity.Title, cancellationToken);
        }

        var persisted = await _invoiceRepository.GetInvoiceByIdAsync(
            createdInvoice.UserId,
            createdInvoice.InvoiceId,
            cancellationToken);

        bool added = persisted is not null
            && persisted.Client is not null
            && persisted.InvoicePositions is not null;

        return added
            ? persisted?.ToDto()
            : throw new InvalidOperationException("Invoice was saved but could not be reloaded.");
    }

    private static readonly string[] AllowedVatRates = { "23%", "8%", "5%", "0%", "zw", "np" };

    private static void ValidateInvoiceParametr(CreateInvoiceDto parametr)
    {
        ArgumentNullException.ThrowIfNull(parametr);

        if (parametr.InvoicePositions is null || parametr.InvoicePositions.Count == 0)
            throw new InvalidOperationException("Invoice must contain at least one position.");

        if (parametr.ClientId is null && (parametr.Client is null || IsClientDtoEmpty(parametr.Client)))
            throw new InvalidOperationException("Invoice must contain clientId or Client details.");

        foreach (var position in parametr.InvoicePositions)
        {            
            if (position.Product is null && position.ProductId is null)
                throw new InvalidOperationException("InvoicePosition must contain Product or ProductId details.");
         
            if (string.IsNullOrWhiteSpace(position.VatRate))
                throw new InvalidOperationException($"VatRate cannot be empty for product: {position.ProductName}");

            if (!AllowedVatRates.Contains(position.VatRate))
                throw new InvalidOperationException($"Invalid VatRate: {position.VatRate}. Allowed values are: {string.Join(", ", AllowedVatRates)}");
            
            if (position.Quantity <= 0)
                throw new InvalidOperationException($"Quantity must be greater than 0 for product: {position.ProductName}");
        }
    }

    private async Task<string> GenerateInvoiceNumberAsync(int userId, IInvoiceRepository invoiceRepository, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        int count = await invoiceRepository.GetInvoicesCountInMonthAsync(userId, now.Month, now.Year, ct);
        int nextNumber = count + 1;

        return $"{nextNumber}/{now.Month:D2}/{now.Year}";
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