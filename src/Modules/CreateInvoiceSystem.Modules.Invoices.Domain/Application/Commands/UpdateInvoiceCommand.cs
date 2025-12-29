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
            throw new ArgumentNullException(nameof(_invoiceRepository));        

        var invoice = await _invoiceRepository.GetInvoiceByIdAsync(
            this.Parametr.InvoiceId,
            includeClient: false,
            includeClientAddress: false,
            includePositions: true,
            includeProducts: false,
            cancellationToken) 
            ?? throw new InvalidOperationException($"Invoice with ID {this.Parametr.InvoiceId} not found.");

        var oldHeader = new 
        {
            invoice.Title,
            invoice.TotalAmount,
            invoice.PaymentDate,
            invoice.CreatedDate,
            invoice.Comments,
            invoice.MethodOfPayment,
            invoice.ClientName,
            invoice.ClientAddress,
            invoice.ClientNip
        };

        var oldPositions = invoice.InvoicePositions
            .Select(ip => new { ip.InvoicePositionId, ip.ProductName, ip.ProductDescription, ip.ProductValue, ip.Quantity })
            .OrderBy(x => x.InvoicePositionId)
            .ToList();

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
            ICollection<UpdateInvoicePositionDto> incoming = this.Parametr.InvoicePositions;

            Dictionary<int, UpdateInvoicePositionDto> incomingById = incoming
                .Where(p => p.InvoicePositionId > 0)
                .ToDictionary(p => p.InvoicePositionId);

            foreach (var invoicePosition in invoice.InvoicePositions.ToList())
            {
                if (incomingById.TryGetValue(invoicePosition.InvoicePositionId, out var dto))
                {                    
                    invoicePosition.ProductName = dto.ProductName ?? invoicePosition.ProductName;
                    invoicePosition.ProductDescription = dto.ProductDescription ?? invoicePosition.ProductDescription;
                    invoicePosition.ProductValue = dto.ProductValue ?? invoicePosition.ProductValue;
                    invoicePosition.Quantity = dto.Quantity;
                }
            }
            
            HashSet<int> incomingIds = [.. incoming.Where(p => p.InvoicePositionId > 0).Select(p => p.InvoicePositionId)];

            List<InvoicePosition> invoicePositionsToDelete = [.. invoice.InvoicePositions.Where(ip => !incomingIds.Contains(ip.InvoicePositionId))];

            foreach (var invoicePositionToDelete in invoicePositionsToDelete)
            {
                await _invoiceRepository.RemoveInvoicePositionsAsync(invoicePositionToDelete);                
            }
            
            List<UpdateInvoicePositionDto> inovoicePositionsToAdd = [.. incoming.Where(p => p.InvoicePositionId == 0)];

            foreach (var inovoicePositionToAdd in inovoicePositionsToAdd)
            {
                var product = await GetOrCreateProductAsync(
                    inovoicePositionToAdd.ProductName,
                    inovoicePositionToAdd.ProductDescription,
                    inovoicePositionToAdd.ProductValue,
                    invoice.UserId,
                    _invoiceRepository,
                    cancellationToken
                     );

                var newInovicePosition = new InvoicePosition
                {
                    InvoiceId = invoice.InvoiceId,
                    ProductId = product.ProductId,
                    Product = product,
                    ProductName = product.Name,
                    ProductDescription = product.Description,
                    ProductValue = product.Value,
                    Quantity = inovoicePositionToAdd.Quantity
                };                
                
                await _invoiceRepository.AddInvoicePositionAsync(newInovicePosition, cancellationToken);
            }
        }

        await _invoiceRepository.SaveChangesAsync(cancellationToken);

        var persisted = await _invoiceRepository.GetInvoiceByIdAsync(
            invoice.InvoiceId,
            includeClient: false,
            includeClientAddress: false,
            includePositions: true,
            includeProducts: false,
            cancellationToken);

        bool headerChanged =
            !string.Equals(oldHeader.Title, persisted.Title, StringComparison.Ordinal) ||
            oldHeader.TotalAmount != persisted.TotalAmount ||
            oldHeader.PaymentDate != persisted.PaymentDate ||
            oldHeader.CreatedDate != persisted.CreatedDate ||
            !string.Equals(oldHeader.Comments, persisted.Comments, StringComparison.Ordinal) ||
            !string.Equals(oldHeader.MethodOfPayment, persisted.MethodOfPayment, StringComparison.Ordinal) ||
            !string.Equals(oldHeader.ClientName, persisted.ClientName, StringComparison.Ordinal) ||
            !string.Equals(oldHeader.ClientAddress, persisted.ClientAddress, StringComparison.Ordinal) ||
            !string.Equals(oldHeader.ClientNip, persisted.ClientNip, StringComparison.Ordinal);
        
        var persistedPositions = persisted.InvoicePositions
            .Select(ip => new { ip.InvoicePositionId, ip.ProductName, ip.ProductDescription, ip.ProductValue, ip.Quantity })
            .OrderBy(x => x.InvoicePositionId)
            .ToList();

        bool positionsChanged =
            oldPositions.Count != persistedPositions.Count ||
            !oldPositions.SequenceEqual(persistedPositions);

        bool hasChanged = headerChanged || positionsChanged;

        return hasChanged
            ? InvoiceMappers.ToUpdateDto(persisted)
            : throw new InvalidOperationException($"No changes were saved for invoice with ID {Parametr.InvoiceId}.");        
    }

    private static async Task<Product> GetOrCreateProductAsync(string productName, string productDescription, decimal? productValue, int userId, IInvoiceRepository _invoiceRepository,   CancellationToken cancellationToken)
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
