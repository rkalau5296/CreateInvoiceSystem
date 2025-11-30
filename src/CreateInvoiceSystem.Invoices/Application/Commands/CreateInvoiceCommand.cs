namespace CreateInvoiceSystem.Invoices.Application.Commands;

using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Abstractions.DbContext;
using CreateInvoiceSystem.Abstractions.Dto;
using CreateInvoiceSystem.Abstractions.Entities;
using CreateInvoiceSystem.Abstractions.Mappers;
using Microsoft.EntityFrameworkCore;
using System.Net;

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

        Invoice entity = null;
        Product product = null;
        Client client = null;        

        if (this.Parametr.ClientId is null)
        {            
            client = ClientMappers.ToEntity(this.Parametr.Client);
            entity = InvoiceMappers.ToInvoiceIfClientIdIsNull(this.Parametr, client);            

            foreach (var position in this.Parametr.InvoicePositions)
            {                
                if (position.ProductId is null)
                {
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
                
                entity.InvoicePositions.Add(new InvoicePosition
                {
                    Name = product.Name,
                    Description = product.Description,
                    Value = product.Value,
                    Quantity = position.Quantity
                });
            }

        }
        else if (this.Parametr.ClientId is not null)
        {
            entity = InvoiceMappers.ToInvoiceIfClientIdIsNotNull(this.Parametr);
            foreach (var position in this.Parametr.InvoicePositions)
            {
                if (position.ProductId is null)
                {
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

                entity.InvoicePositions.Add(new InvoicePosition
                {
                    Name = product.Name,
                    Description = product.Description,
                    Value = product.Value,
                    Quantity = position.Quantity
                });
            }
        }
        
        await context.Set<Client>().AddAsync(client, cancellationToken);
        await context.Set<Invoice>().AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        return this.Parametr;
    }
}
