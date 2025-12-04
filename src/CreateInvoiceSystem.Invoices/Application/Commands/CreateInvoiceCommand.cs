namespace CreateInvoiceSystem.Invoices.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;
using Microsoft.EntityFrameworkCore;

public class CreateInvoiceCommand : CommandBase<CreateInvoiceDto, CreateInvoiceDto>
{
    public override async Task<CreateInvoiceDto> Execute(IDbContext context, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(context));

        if (this.Parametr.InvoicePositions == null || this.Parametr.InvoicePositions.Count == 0)
        {
            throw new InvalidOperationException("Invoice must contain at least one position.");
        }

        Invoice entity = new();

        if (this.Parametr.ClientId is null)
        {
            Client client = new();
            client = ClientMappers.ToEntity(this.Parametr.Client);
            client.UserId = this.Parametr.UserId;
            entity = InvoiceMappers.ToInvoiceWithNewClient(this.Parametr, client);
            await context.Set<Client>().AddAsync(client, cancellationToken);

            foreach (var position in this.Parametr.InvoicePositions)
            {              
                Product product = new();
                if (position.ProductId is null)
                {                    
                    product.UserId = this.Parametr.UserId;
                    product.Name = position.Product.Name;
                    product.Description = position.Product.Description;
                    product.Value = position.Product.Value;                        
                    await context.Set<Product>().AddAsync(product, cancellationToken);                    
                }
                if (position.ProductId is not null)
                {
                    product = await context.Set<Product>().FirstOrDefaultAsync(c => c.ProductId == position.ProductId, cancellationToken: cancellationToken)
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
        else if (this.Parametr.ClientId is not null)
        {
            Client client = new();
            
            client = await context.Set<Client>()
                .Include(c =>c.Address)
                .FirstOrDefaultAsync(c => c.ClientId == this.Parametr.ClientId, cancellationToken: cancellationToken)
                        ?? throw new InvalidOperationException($"Client with ID {this.Parametr.ClientId} not found.");
            entity = InvoiceMappers.ToInvoiceWithExistingClient(this.Parametr, client);

            foreach (var position in this.Parametr.InvoicePositions)
            {
                Product product = new();
                if (position.ProductId is null)
                {
                    product.UserId = this.Parametr.UserId;
                    product.Name = position.Product.Name;
                    product.Description = position.Product.Description;
                    product.Value = position.Product.Value;
                    await context.Set<Product>().AddAsync(product, cancellationToken);                    
                }
                if (position.ProductId is not null)
                {
                    product = await context.Set<Product>().FirstOrDefaultAsync(c => c.ProductId == position.ProductId, cancellationToken: cancellationToken)
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
        
        await context.Set<Invoice>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return this.Parametr;
    }    
}
