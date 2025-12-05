namespace CreateInvoiceSystem.Invoices.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;
using Microsoft.EntityFrameworkCore;
using System.Threading;

public class CreateInvoiceCommand : CommandBase<CreateInvoiceDto, CreateInvoiceDto>
{
    public override async Task<CreateInvoiceDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        var param = this.Parametr ?? throw new ArgumentNullException(nameof(Parametr));

        if (this.Parametr.InvoicePositions is null || this.Parametr.InvoicePositions.Count == 0)throw new InvalidOperationException("Invoice must contain at least one position.");       

        if(this.Parametr.ClientId is null && this.Parametr.Client is null)
            throw new InvalidOperationException("Invoice must contain clientId or Client details.");      

        Invoice entity = new();
        
        if (this.Parametr.ClientId is null)
        {
            Client client = new();
            Client existingClient = await CheckWhetherClientExists(this.Parametr, context, cancellationToken);

            if (existingClient is null)
            {
                client = ClientMappers.ToEntity(this.Parametr.Client);                
                entity = InvoiceMappers.ToInvoiceWithNewClient(this.Parametr, client);
                await context.Set<Client>().AddAsync(client, cancellationToken);
            }
            else
            {                
                entity = InvoiceMappers.ToInvoiceWithNewClient(this.Parametr, existingClient);
            }                

            await AddProductToInvoicePosition(this.Parametr, entity, context, cancellationToken);
        }
        else if (this.Parametr.ClientId is not null)
        {
            Client client = new();            
            client = await context.Set<Client>()
                .Include(c =>c.Address)
                .FirstOrDefaultAsync(c => c.ClientId == this.Parametr.ClientId, cancellationToken: cancellationToken)
                        ?? throw new InvalidOperationException($"Client with ID {this.Parametr.ClientId} not found.");
            entity = InvoiceMappers.ToInvoiceWithExistingClient(this.Parametr, client);

            await AddProductToInvoicePosition(this.Parametr, entity, context, cancellationToken);
        }
        
        await context.Set<Invoice>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return this.Parametr;
    } 
    
    private static async Task AddProductToInvoicePosition(CreateInvoiceDto parametr, Invoice entity, IDbContext context, CancellationToken cancellationToken)
    {
        foreach (var position in parametr.InvoicePositions)
        {
            Product product = new();
            if (position.ProductId is null)
            {                
                var existingProduct = await context.Set<Product>().FirstOrDefaultAsync(p =>
                    p.Name == position.Product.Name &&
                    p.Description == position.Product.Description &&
                    p.Value == position.Product.Value &&
                    p.UserId == parametr.UserId, cancellationToken);

                if (existingProduct is null)
                {
                    product = new Product
                    {
                        UserId = parametr.UserId,
                        Name = position.Product.Name,
                        Description = position.Product.Description,
                        Value = position.Product.Value
                    };
                    await context.Set<Product>().AddAsync(product, cancellationToken);
                }
                else
                {
                    product = existingProduct;
                }
            }
            else
            {
                product = await context.Set<Product>().FirstOrDefaultAsync(c => c.ProductId == position.ProductId, cancellationToken)
                    ?? throw new InvalidOperationException($"Product with ID {position.ProductId} not found.");
            }

            InvoicePosition invoicePosition = new()
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

    private static async Task<Client> CheckWhetherClientExists(CreateInvoiceDto dto, IDbContext context, CancellationToken cancellationToken)
    {
         return await context.Set<Client>()
                .Include(c => c.Address)
                .FirstOrDefaultAsync(c =>
            c.Nip == dto.Client.Nip &&
            c.Name == dto.Client.Name &&
            c.IsDeleted == false &&
            c.Address.Street == dto.Client.Address.Street &&
            c.Address.Number == dto.Client.Address.Number &&
            c.Address.City == dto.Client.Address.City &&
            c.Address.PostalCode == dto.Client.Address.PostalCode &&
            c.Address.Country == dto.Client.Address.Country,            
            cancellationToken);
    }
}