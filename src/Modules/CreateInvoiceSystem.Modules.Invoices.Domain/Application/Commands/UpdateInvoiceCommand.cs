using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Commands;

public class UpdateInvoiceCommand : CommandBase<UpdateInvoiceDto, UpdateInvoiceDto, IInvoiceRepository>
{
    public override async Task<UpdateInvoiceDto> Execute(IInvoiceRepository _invoiceRepository, CancellationToken cancellationToken = default)
    {
        if (this.Parametr is null)
            throw new ArgumentNullException(nameof(this.Parametr));
        
        var invoice = await _invoiceRepository.GetInvoiceByIdAsync(
            this.Parametr.InvoiceId,
            cancellationToken)
            ?? throw new InvalidOperationException($"Invoice with ID {this.Parametr.InvoiceId} not found.");
        
        invoice.Title = this.Parametr.Title ?? invoice.Title;
        invoice.TotalAmount = this.Parametr.TotalAmount != default ? this.Parametr.TotalAmount : invoice.TotalAmount;
        invoice.PaymentDate = this.Parametr.PaymentDate != default ? this.Parametr.PaymentDate : invoice.PaymentDate;
        invoice.CreatedDate = this.Parametr.CreatedDate != default ? this.Parametr.CreatedDate : invoice.CreatedDate;
        invoice.Comments = this.Parametr.Comments ?? invoice.Comments;
        invoice.MethodOfPayment = this.Parametr.MethodOfPayment ?? invoice.MethodOfPayment;
        invoice.ClientName = this.Parametr.ClientName ?? invoice.ClientName;
        invoice.ClientAddress = this.Parametr.ClientAddress ?? invoice.ClientAddress;
        invoice.ClientNip = this.Parametr.ClientNip ?? invoice.ClientNip;
        
        if (this.Parametr.InvoicePositions is not null)
        {
            var incomingPositions = this.Parametr.InvoicePositions;
            
            var incomingIds = incomingPositions
                .Where(p => p.InvoicePositionId > 0)
                .Select(p => p.InvoicePositionId)
                .ToHashSet();
            
            var positionsToDelete = invoice.InvoicePositions
                .Where(ip => !incomingIds.Contains(ip.InvoicePositionId))
                .ToList();

            foreach (var toDelete in positionsToDelete)
            {
                invoice.InvoicePositions.Remove(toDelete);
                await _invoiceRepository.RemoveInvoicePositionsAsync(toDelete);
            }
            
            foreach (var dto in incomingPositions)
            {
                if (dto.InvoicePositionId > 0)
                {                    
                    var existingPos = invoice.InvoicePositions
                        .FirstOrDefault(ip => ip.InvoicePositionId == dto.InvoicePositionId);

                    if (existingPos != null)
                    {
                        existingPos.ProductName = dto.ProductName ?? existingPos.ProductName;
                        existingPos.ProductDescription = dto.ProductDescription ?? existingPos.ProductDescription;
                        existingPos.ProductValue = dto.ProductValue ?? existingPos.ProductValue;
                        existingPos.Quantity = dto.Quantity;
                    }
                }
                else
                {                    
                    var product = await GetOrCreateProductAsync(
                        dto.ProductName,
                        dto.ProductDescription,
                        dto.ProductValue,
                        invoice.UserId,
                        _invoiceRepository,
                        cancellationToken);

                    var newPosition = new InvoicePosition
                    {
                        InvoiceId = invoice.InvoiceId,
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        ProductDescription = product.Description,
                        ProductValue = product.Value,
                        Quantity = dto.Quantity
                    };
                    
                    invoice.InvoicePositions.Add(newPosition);
                }
            }
        }
        
        await _invoiceRepository.SaveChangesAsync(cancellationToken);
        
        var updatedInvoice = await _invoiceRepository.GetInvoiceByIdAsync(
            this.Parametr.InvoiceId,
            cancellationToken)
            ?? throw new InvalidOperationException($"Invoice with ID {this.Parametr.InvoiceId} not found after update.");

        bool isChanged =
        invoice.Title != updatedInvoice.Title ||
        invoice.TotalAmount != updatedInvoice.TotalAmount ||
        invoice.PaymentDate != updatedInvoice.PaymentDate ||
        invoice.ClientName != updatedInvoice.ClientName ||
        invoice.InvoicePositions.Count != updatedInvoice.InvoicePositions.Count;        

        return isChanged
        ? InvoiceMappers.ToUpdateDto(updatedInvoice)
        : throw new Exception("The invoice has not changed.");
    }    

    private static async Task<Product> GetOrCreateProductAsync(
        string productName,
        string productDescription,
        decimal? productValue,
        int userId,
        IInvoiceRepository _invoiceRepository,
        CancellationToken cancellationToken)
    {
        var existing = await _invoiceRepository.GetProductAsync(productName, productDescription, productValue, userId, cancellationToken);

        if (existing is not null)
            return existing;

        var newProduct = new Product
        {
            UserId = userId,
            Name = productName,
            Description = productDescription,
            Value = productValue
        };

        await _invoiceRepository.AddProductAsync(newProduct, cancellationToken);

        
        return newProduct;
    }
}