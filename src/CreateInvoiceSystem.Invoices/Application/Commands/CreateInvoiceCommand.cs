namespace CreateInvoiceSystem.Invoices.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;
using Microsoft.EntityFrameworkCore;
using System.Threading;

public class CreateInvoiceCommand : CommandBase<CreateInvoiceDto, InvoiceDto>
{
    public override async Task<InvoiceDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        ValidateInvoiceParametr(this.Parametr);

        Invoice entity;       

        Client client = this.Parametr.ClientId is null
            ? await GetOrCreateClientAsync(this.Parametr, context, cancellationToken)
            : await GetClientByIdAsync(this.Parametr.ClientId.Value, context, cancellationToken);

        entity = this.Parametr.ClientId is null
            ? InvoiceMappers.ToInvoiceWithNewClient(this.Parametr, client)
            : InvoiceMappers.ToInvoiceWithExistingClient(this.Parametr, client);

        await AddProductsToInvoicePositionsAsync(this.Parametr, entity, context, cancellationToken);

        await context.Set<Invoice>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return InvoiceMappers.ToDto(entity);
    }

    private static void ValidateInvoiceParametr(CreateInvoiceDto parametr)
    {
        ArgumentNullException.ThrowIfNull(parametr);

        if (parametr.InvoicePositions is null || parametr.InvoicePositions.Count == 0)
            throw new InvalidOperationException("Invoice must contain at least one position.");

        if (parametr.ClientId is null && parametr.Client is null)
            throw new InvalidOperationException("Invoice must contain clientId or Client details.");

        if (parametr.InvoicePositions.Any(ip => ip.Product is null && ip.ProductId is null))
            throw new InvalidOperationException("InvoicePosition must contain Product or ProductId details.");
    }

    private static async Task<Client> GetOrCreateClientAsync(CreateInvoiceDto param, IDbContext context, CancellationToken cancellationToken)
    {
        var client = await context.Set<Client>()
            .Include(c => c.Address)
            .FirstOrDefaultAsync(c =>
                c.Nip == param.Client.Nip &&
                c.Name == param.Client.Name &&
                c.IsDeleted == false &&
                c.Address.Street == param.Client.Address.Street &&
                c.Address.Number == param.Client.Address.Number &&
                c.Address.City == param.Client.Address.City &&
                c.Address.PostalCode == param.Client.Address.PostalCode &&
                c.Address.Country == param.Client.Address.Country, cancellationToken);

        if (client is not null)
            return client;

        var newClient = ClientMappers.ToEntity(param.Client);
        newClient.UserId = param.UserId;
        await context.Set<Client>().AddAsync(newClient, cancellationToken);
        return newClient;
    }

    private static async Task<Client> GetClientByIdAsync(int clientId, IDbContext context, CancellationToken cancellationToken)
    {
        return await context.Set<Client>()
            .Include(c => c.Address)
            .FirstOrDefaultAsync(c => c.ClientId == clientId, cancellationToken)
            ?? throw new InvalidOperationException($"Client with ID {clientId} not found.");
    }

    private static async Task AddProductsToInvoicePositionsAsync(CreateInvoiceDto param, Invoice entity, IDbContext context, CancellationToken cancellationToken)
    {
        foreach (var position in param.InvoicePositions)
        {
            var product = position.ProductId is null
                ? await GetOrCreateProductAsync(position, param.UserId, context, cancellationToken)
                : await GetProductByIdAsync(position.ProductId.Value, context, cancellationToken);

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
            await context.Set<InvoicePosition>().AddAsync(invoicePosition, cancellationToken);
        }
    }

    private static async Task<Product> GetOrCreateProductAsync(InvoicePositionDto position, int userId, IDbContext context, CancellationToken cancellationToken)
    {
        var existing = await context.Set<Product>().FirstOrDefaultAsync(p =>
            p.Name == position.Product.Name &&
            p.Description == position.Product.Description &&
            p.Value == position.Product.Value &&
            p.UserId == userId, cancellationToken);

        if (existing is not null) return existing;

        var newProduct = new Product
        {
            UserId = userId,
            Name = position.Product.Name,
            Description = position.Product.Description,
            Value = position.Product.Value
        };
        await context.Set<Product>().AddAsync(newProduct, cancellationToken);
        return newProduct;
    }
    
    private static async Task<Product> GetProductByIdAsync(int productId, IDbContext context, CancellationToken cancellationToken)
    {
        return await context.Set<Product>()
            .FirstOrDefaultAsync(p => p.ProductId == productId, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {productId} not found.");
    }    
}