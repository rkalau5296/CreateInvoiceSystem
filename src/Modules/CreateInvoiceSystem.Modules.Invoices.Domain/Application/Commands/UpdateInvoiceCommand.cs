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
            throw new ArgumentNullException(nameof(Parametr));

        var beforeUpdate = await _invoiceRepository.GetInvoiceByIdAsync(this.Parametr.UserId, this.Parametr.InvoiceId, cancellationToken)
            ?? throw new InvalidOperationException($"Invoice {this.Parametr.InvoiceId} not found.");

        var invoice = await _invoiceRepository.GetInvoiceByIdAsync(this.Parametr.UserId, this.Parametr.InvoiceId, cancellationToken)
            ?? throw new InvalidOperationException($"Invoice {this.Parametr.InvoiceId} not found.");

        UpdateBasicInformation(invoice);

        await HandleClientUpdate(invoice, _invoiceRepository, cancellationToken);

        if (this.Parametr.InvoicePositions != null)
        {
            await SyncInvoicePositions(invoice, _invoiceRepository, cancellationToken);
        }

        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);

        var updatedInvoice = await _invoiceRepository.GetInvoiceByIdAsync(this.Parametr.UserId, this.Parametr.InvoiceId, cancellationToken)
            ?? throw new InvalidOperationException("Invoice not found after update.");

        return HasChanges(beforeUpdate, updatedInvoice)
            ? InvoiceMappers.ToUpdateDto(updatedInvoice)
            : throw new Exception("The invoice has not changed.");
    }

    private void UpdateBasicInformation(Invoice invoice)
    {
        invoice.Title = Parametr.Title ?? invoice.Title;
        invoice.TotalAmount = Parametr.TotalAmount != default ? Parametr.TotalAmount : invoice.TotalAmount;
        invoice.PaymentDate = Parametr.PaymentDate != default ? Parametr.PaymentDate : invoice.PaymentDate;
        invoice.CreatedDate = Parametr.CreatedDate != default ? Parametr.CreatedDate : invoice.CreatedDate;
        invoice.Comments = Parametr.Comments ?? invoice.Comments;
        invoice.MethodOfPayment = Parametr.MethodOfPayment ?? invoice.MethodOfPayment;
    }

    private async Task HandleClientUpdate(Invoice invoice, IInvoiceRepository _invoiceRepository, CancellationToken cancellationToken)
    {
        if (Parametr.Client != null)
        {
            var existingClient = await _invoiceRepository.GetClientAsync(
                Parametr.Client.Name,
                Parametr.Client.Address.Street,
                Parametr.Client.Address.Number,
                Parametr.Client.Address.City,
                Parametr.Client.Address.PostalCode,
                Parametr.Client.Address.Country,
                invoice.UserId,
                cancellationToken);

            if (existingClient != null)
            {
                invoice.Client = existingClient;
                invoice.ClientId = existingClient.ClientId;
            }
            else
            {
                var newClient = MapToNewClient(Parametr.Client, invoice.UserId);
                await _invoiceRepository.AddClientAsync(newClient, cancellationToken);
                invoice.Client = newClient;
                invoice.ClientId = newClient.ClientId;
            }

            invoice.ClientName = Parametr.Client.Name;
            invoice.ClientNip = Parametr.Client.Nip;
            invoice.ClientAddress = FormatAddress(Parametr.Client.Address);
        }
        else if (Parametr.ClientId.HasValue && Parametr.ClientId.Value > 0)
        {
            var existing = await _invoiceRepository.GetClientByIdAsync(Parametr.ClientId.Value, cancellationToken)
                ?? throw new InvalidOperationException($"Client with ID {Parametr.ClientId.Value} not found.");

            invoice.Client = existing;
            invoice.ClientId = existing.ClientId;
            invoice.ClientName = existing.Name;
            invoice.ClientNip = existing.Nip;
            invoice.ClientAddress = FormatAddress(existing.Address);
        }
    }

    private async Task SyncInvoicePositions(Invoice invoice, IInvoiceRepository _invoiceRepository, CancellationToken cancellationToken)
    {
        var incoming = Parametr.InvoicePositions;
        var incomingIds = incoming.Where(p => p.InvoicePositionId > 0).Select(p => p.InvoicePositionId).ToHashSet();

        var toDelete = invoice.InvoicePositions
            .Where(ip => !incomingIds.Contains(ip.InvoicePositionId))
            .ToList();

        foreach (var pos in toDelete)
        {
            invoice.InvoicePositions.Remove(pos);
            await _invoiceRepository.RemoveInvoicePositionsAsync(pos);
        }

        foreach (var dto in incoming)
        {            
            var nameToUse = dto.Product?.Name ?? dto.ProductName;
            var descToUse = dto.Product?.Description ?? dto.ProductDescription;
            var valToUse = dto.Product?.Value ?? dto.ProductValue;
            
            var product = await GetOrCreateProductAsync(nameToUse, descToUse, valToUse, invoice.UserId, _invoiceRepository, cancellationToken);

            if (dto.InvoicePositionId > 0)
            {                
                var existing = invoice.InvoicePositions.FirstOrDefault(p => p.InvoicePositionId == dto.InvoicePositionId);
                if (existing != null)
                {
                    existing.ProductId = product.ProductId; 
                    existing.ProductName = product.Name;
                    existing.ProductDescription = product.Description;
                    existing.ProductValue = product.Value;
                    existing.Quantity = dto.Quantity;
                }
            }
            else
            {                
                invoice.InvoicePositions.Add(new InvoicePosition
                {
                    InvoiceId = invoice.InvoiceId,
                    ProductId = product.ProductId,
                    ProductName = product.Name,
                    ProductDescription = product.Description,
                    ProductValue = product.Value,
                    Quantity = dto.Quantity
                });
            }
        }
    }
    

    private static bool HasChanges(Invoice before, Invoice after)
    {
        return before.Title != after.Title ||
               before.TotalAmount != after.TotalAmount ||
               before.PaymentDate != after.PaymentDate ||
               before.ClientName != after.ClientName ||
               before.ClientId != after.ClientId ||
               before.InvoicePositions.Count != after.InvoicePositions.Count ||
               !before.InvoicePositions.Select(p => p.ProductName + p.Quantity + p.ProductValue).SequenceEqual(after.InvoicePositions.Select(p => p.ProductName + p.Quantity + p.ProductValue));
    }

    private static string FormatAddress(dynamic addr) => addr == null ? "" : $"{addr.Street} {addr.Number}, {addr.PostalCode} {addr.City}, {addr.Country}";

    private static Client MapToNewClient(UpdateClientDto dto, int userId) => new()
    {
        ClientId = 0,
        Name = dto.Name,
        Nip = dto.Nip,
        UserId = userId,
        Address = new Address
        {
            Street = dto.Address.Street,
            Number = dto.Address.Number,
            City = dto.Address.City,
            PostalCode = dto.Address.PostalCode,
            Country = dto.Address.Country
        }
    };

    private static async Task<Product> GetOrCreateProductAsync(string name, string desc, decimal? val, int userId, IInvoiceRepository repo, CancellationToken ct)
    {
        var existing = await repo.GetProductAsync(name, desc, val, userId, ct);
        if (existing != null) return existing;

        var newProduct = new Product { UserId = userId, Name = name, Description = desc, Value = val };
        await repo.AddProductAsync(newProduct, ct);
        return newProduct;
    }
}