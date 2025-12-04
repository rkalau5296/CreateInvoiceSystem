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
        Product product = new();
        Client client = null;        

        if (this.Parametr.ClientId is null)
        {            
            client = ClientMappers.ToEntity(this.Parametr.Client);
            client.UserId = this.Parametr.UserId;
            entity = InvoiceMappers.ToInvoiceIfClientIdIsNull(this.Parametr, client);
            await context.Set<Client>().AddAsync(client, cancellationToken);

            foreach (var position in this.Parametr.InvoicePositions)
            {                
                if (position.ProductId is null)
                {
                    product.Name = position.Name;
                    product.Description = position.Description;
                    product.Value = position.Value;
                    product.UserId = this.Parametr.UserId;
                    await context.Set<Product>().AddAsync(product, cancellationToken);
                }
                if (position.ProductId is not null)
                {
                    product = await context.Set<Product>().FirstOrDefaultAsync(c => c.ProductId == position.ProductId, cancellationToken: cancellationToken)
                        ?? throw new InvalidOperationException($"Product with ID {position.ProductId} not found.");                   
                }

                InvoicePosition invoicePosition = new()
                {
                    Name = product.Name,
                    Description = product.Description,
                    Value = product.Value,
                    Quantity = position.Quantity,
                    Product = product
                };
                entity.InvoicePositions.Add(invoicePosition);
                await context.Set<InvoicePosition>().AddAsync(invoicePosition, cancellationToken);
            }

        }
        else if (this.Parametr.ClientId is not null)
        {
            entity = InvoiceMappers.ToInvoiceIfClientIdIsNotNull(this.Parametr);
            entity.Client = await context.Set<Client>().FirstOrDefaultAsync(c => c.ClientId == this.Parametr.ClientId, cancellationToken: cancellationToken)
                        ?? throw new InvalidOperationException($"Product with ID {this.Parametr.ClientId} not found.");

            foreach (var position in this.Parametr.InvoicePositions)
            {
                if (position.ProductId is null)
                {
                    product.Name = position.Name;
                    product.Description = position.Description;
                    product.Value = position.Value;
                    product.UserId = this.Parametr.UserId;
                    await context.Set<Product>().AddAsync(product, cancellationToken);
                }
                if (position.ProductId is not null)
                {
                    product = await context.Set<Product>().FirstOrDefaultAsync(c => c.ProductId == position.ProductId, cancellationToken: cancellationToken)
                        ?? throw new InvalidOperationException($"Product with ID {position.ProductId} not found.");
                }

                InvoicePosition invoicePosition = new()
                {
                    Name = product.Name,
                    Description = product.Description,
                    Value = product.Value,
                    Quantity = position.Quantity                    
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
