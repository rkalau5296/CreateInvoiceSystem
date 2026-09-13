using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using CreateInvoiceSystem.Modules.Invoices.Domain.Entities;
using CreateInvoiceSystem.Modules.Invoices.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Domain.Mappers;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.Commands;

public class UpdateInvoiceCommand : CommandBase<UpdateInvoiceDto, UpdateInvoiceDto, IInvoiceRepository>
{
    public override async Task<UpdateInvoiceDto> Execute(IInvoiceRepository invoiceRepository, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(Parametr);

        var invoice = await invoiceRepository.GetInvoiceByIdAsync(
            Parametr.UserId,
            Parametr.InvoiceId,
            cancellationToken)
            ?? throw new InvalidOperationException(
                $"Invoice {Parametr.InvoiceId} not found.");

        UpdateBasicInformation(invoice);

        await HandleClientUpdate(invoice, invoiceRepository, cancellationToken);

        if (Parametr.InvoicePositions is not null)
        {
            await SyncInvoicePositions(invoice, invoiceRepository, cancellationToken);
        }

        await invoiceRepository.UpdateAsync(invoice, cancellationToken);

        return InvoiceMappers.ToUpdateDto(invoice);
    }

    private void UpdateBasicInformation(Invoice invoice)
    {
        invoice.Title = Parametr.Title ?? invoice.Title;
        invoice.TotalNet = Parametr.TotalNet != default
            ? Parametr.TotalNet
            : invoice.TotalNet;
        invoice.TotalVat = Parametr.TotalVat != default
            ? Parametr.TotalVat
            : invoice.TotalVat;
        invoice.TotalGross = Parametr.TotalGross != default
            ? Parametr.TotalGross
            : invoice.TotalGross;
        invoice.PaymentDate = Parametr.PaymentDate != default
            ? Parametr.PaymentDate
            : invoice.PaymentDate;
        invoice.CreatedDate = Parametr.CreatedDate != default
            ? Parametr.CreatedDate
            : invoice.CreatedDate;
        invoice.Comments = Parametr.Comments ?? invoice.Comments;
        invoice.MethodOfPayment =
            Parametr.MethodOfPayment ?? invoice.MethodOfPayment;

        invoice.ClientAddress =
            Parametr.ClientAddress ?? invoice.ClientAddress;
        invoice.ClientName =
            Parametr.ClientName ?? invoice.ClientName;
        invoice.ClientNip =
            Parametr.ClientNip ?? invoice.ClientNip;
        invoice.ClientEmail =
            Parametr.ClientEmail ?? invoice.ClientEmail;

        invoice.SellerName =
            Parametr.SellerName ?? invoice.SellerName;
        invoice.SellerNip =
            Parametr.SellerNip ?? invoice.SellerNip;
        invoice.SellerAddress =
            Parametr.SellerAddress ?? invoice.SellerAddress;
        invoice.BankAccountNumber =
            Parametr.BankAccountNumber ?? invoice.BankAccountNumber;
    }

    private async Task HandleClientUpdate(Invoice invoice, IInvoiceRepository invoiceRepository, CancellationToken cancellationToken)
    {
        if (Parametr.Client is not null)
        {
            var clientDto = Parametr.Client;

            var existingClient = await invoiceRepository.GetClientAsync(
                clientDto.Name,
                clientDto.Address.Street,
                clientDto.Address.Number,
                clientDto.Address.City,
                clientDto.Address.PostalCode,
                clientDto.Address.Country,
                invoice.UserId,
                clientDto.Email,
                cancellationToken);

            if (existingClient is null)
            {
                var newClient = MapToNewClient(
                    clientDto,
                    invoice.UserId);

                await invoiceRepository.AddClientAsync(
                    newClient,
                    cancellationToken);
            }

            invoice.ClientName = clientDto.Name;
            invoice.ClientNip = clientDto.Nip;
            invoice.ClientEmail = clientDto.Email;
            invoice.ClientAddress = FormatAddress(clientDto.Address);

            return;
        }

        if (Parametr.ClientId is not > 0)
        {
            return;
        }

        var existingClientById =
            await invoiceRepository.GetClientByIdAsync(Parametr.ClientId.Value, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Client with ID {Parametr.ClientId.Value} not found.");

        invoice.ClientName = existingClientById.Name;
        invoice.ClientNip = existingClientById.Nip;
        invoice.ClientEmail = existingClientById.Email;
        invoice.ClientAddress =
            FormatAddress(existingClientById.Address);
    }

    private async Task SyncInvoicePositions(Invoice invoice, IInvoiceRepository invoiceRepository, CancellationToken cancellationToken)
    {
        var incomingPositions = Parametr.InvoicePositions!;

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

            await invoiceRepository.RemoveInvoicePositionsAsync(
                position);
        }

        foreach (var incomingPosition in incomingPositions)
        {
            var product = await GetOrCreateProductAsync(
                incomingPosition.ProductName,
                incomingPosition.ProductDescription,
                incomingPosition.ProductValue,
                invoice.UserId,
                invoiceRepository,
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

    private static string FormatAddress(dynamic address)
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