using CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.UpdateInvoice;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Handlers;
public class UpdateInvoiceHandler(IInvoiceRepository _invoiceRepository) : IRequestHandler<UpdateInvoiceRequest, UpdateInvoiceResponse>
{    
    public async Task<UpdateInvoiceResponse> Handle(UpdateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoice = await _invoiceRepository.GetInvoiceByIdAsync(
            request.UserId,
            request.Id,
            cancellationToken)
            ?? throw new InvalidOperationException(
                $"Invoice {request.Id} not found.");

        UpdateBasicInformation(invoice, request);

        await HandleClientUpdate(invoice, request, cancellationToken);

        if (request.Invoice.InvoicePositions is not null)
        {
            await SyncInvoicePositions(invoice, request, cancellationToken);
        }

        await _invoiceRepository.UpdateAsync(invoice, cancellationToken);
                
        return new UpdateInvoiceResponse()
        {
            Data = InvoiceMappers.ToUpdateDto(invoice)
        };
    }
    private void UpdateBasicInformation(Invoice invoice, UpdateInvoiceRequest request)
    {
        invoice.Title = request.Invoice.Title ?? invoice.Title;

        invoice.TotalNet = request.Invoice.TotalNet ?? invoice.TotalNet;
        invoice.TotalVat = request.Invoice.TotalVat ?? invoice.TotalVat;
        invoice.TotalGross = request.Invoice.TotalGross ?? invoice.TotalGross;
        invoice.PaymentDate = request.Invoice.PaymentDate ?? invoice.PaymentDate;
        invoice.CreatedDate = request.Invoice.CreatedDate ?? invoice.CreatedDate;

        invoice.Comments = request.Invoice.Comments ?? invoice.Comments;
        invoice.MethodOfPayment =
            request.Invoice.MethodOfPayment ?? invoice.MethodOfPayment;
        invoice.ClientAddress =
            request.Invoice.ClientAddress ?? invoice.ClientAddress;
        invoice.ClientName =
            request.Invoice.ClientName ?? invoice.ClientName;
        invoice.ClientNip =
            request.Invoice.ClientNip ?? invoice.ClientNip;
        invoice.ClientEmail =
            request.Invoice.ClientEmail ?? invoice.ClientEmail;

        invoice.SellerName =
            request.Invoice.SellerName ?? invoice.SellerName;
        invoice.SellerNip =
            request.Invoice.SellerNip ?? invoice.SellerNip;
        invoice.SellerAddress =
            request.Invoice.SellerAddress ?? invoice.SellerAddress;
        invoice.BankAccountNumber =
            request.Invoice.BankAccountNumber ?? invoice.BankAccountNumber;
    }

    private async Task HandleClientUpdate(Invoice invoice, UpdateInvoiceRequest request, CancellationToken cancellationToken)
    {
        if (request.Invoice.Client is not null)
        {
            var clientDto = request.Invoice.Client;

            var client = await _invoiceRepository.GetClientAsync(
                clientDto.Name,
                clientDto.Address.Street,
                clientDto.Address.Number,
                clientDto.Address.City,
                clientDto.Address.PostalCode,
                clientDto.Address.Country,
                invoice.UserId,
                clientDto.Email,
                cancellationToken);

            if (client is null)
            {
                client = MapToNewClient(
                    clientDto,
                    invoice.UserId);

                await _invoiceRepository.AddClientAsync(
                    client,
                    cancellationToken);
            }

            invoice.ClientId = client.ClientId > 0 ? client.ClientId : invoice.ClientId;
            invoice.ClientName = client.Name;
            invoice.ClientNip = client.Nip;
            invoice.ClientEmail = client.Email;
            invoice.ClientAddress = FormatAddress(clientDto.Address);

            return;
        }

        if (request.Invoice.ClientId is not > 0)
        {
            return;
        }

        var existingClientById =
            await _invoiceRepository.GetClientByIdAsync(request.Invoice.ClientId.Value, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Client with ID {request.Invoice.ClientId.Value} not found.");

        invoice.ClientId = existingClientById.ClientId;
        invoice.ClientName = existingClientById.Name;
        invoice.ClientNip = existingClientById.Nip;
        invoice.ClientEmail = existingClientById.Email;
        invoice.ClientAddress = FormatAddress(existingClientById.Address);
    }

    private async Task SyncInvoicePositions(Invoice invoice, UpdateInvoiceRequest request, CancellationToken cancellationToken)
    {
        var incomingPositions = request.Invoice.InvoicePositions!;

        var incomingIds = incomingPositions
            .Where(position => position.InvoicePositionId > 0)
            .Select(position => position.InvoicePositionId)
            .ToHashSet();

        var positionsToDelete = invoice.InvoicePositions
            .Where(position =>
                !incomingIds.Contains(position.InvoicePositionId))
            .ToList();

        foreach (var position in positionsToDelete)
        {
            invoice.InvoicePositions.Remove(position);

            await _invoiceRepository.RemoveInvoicePositionsAsync(
                position);
        }

        foreach (var incomingPosition in incomingPositions)
        {
            var product = await GetOrCreateProductAsync(
                incomingPosition.ProductName,
                incomingPosition.ProductDescription ?? string.Empty,
                incomingPosition.ProductValue,
                invoice.UserId,
                _invoiceRepository,
                cancellationToken);

            if (incomingPosition.InvoicePositionId > 0)
            {
                var existingPosition = invoice.InvoicePositions
                    .FirstOrDefault(position =>
                        position.InvoicePositionId ==
                        incomingPosition.InvoicePositionId);

                if (existingPosition is null)
                {
                    throw new InvalidOperationException(
                        $"Invoice position "
                        + $"{incomingPosition.InvoicePositionId} "
                        + $"does not belong to invoice "
                        + $"{invoice.InvoiceId}.");
                }

                existingPosition.ProductId = product.ProductId;
                existingPosition.ProductName = product.Name;
                existingPosition.ProductDescription = product.Description;
                existingPosition.ProductValue = product.Value;
                existingPosition.Quantity = incomingPosition.Quantity;
                existingPosition.VatRate = incomingPosition.VatRate;
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
                    Quantity = incomingPosition.Quantity,
                    VatRate = incomingPosition.VatRate
                });
            }
        }
    }

    private static string FormatAddress(AddressDto? address)
    {
        return address is null
            ? string.Empty
            : $"{address.Street} {address.Number}, "
              + $"{address.PostalCode} {address.City}, "
              + address.Country;
    }

    private static string FormatAddress(Address? address)
    {
        return address is null
            ? string.Empty
            : $"{address.Street} {address.Number}, "
              + $"{address.PostalCode} {address.City}, "
              + address.Country;
    }

    private static Client MapToNewClient(
        UpdateClientDto dto,
        int userId)
    {
        return new Client
        {
            ClientId = 0,
            Name = dto.Name,
            Nip = dto.Nip,
            Email = dto.Email,
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
    }

    private static async Task<Product> GetOrCreateProductAsync(
        string name,
        string description,
        decimal? value,
        int userId,
        IInvoiceRepository invoiceRepository,
        CancellationToken cancellationToken)
    {
        var existingProduct = await invoiceRepository.GetProductAsync(
            name,
            description,
            value,
            userId,
            cancellationToken);

        if (existingProduct is not null)
        {
            return existingProduct;
        }

        var newProduct = new Product
        {
            UserId = userId,
            Name = name,
            Description = description,
            Value = value
        };

        await invoiceRepository.AddProductAsync(
            newProduct,
            cancellationToken);

        return newProduct;
    }
}
