using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.CreateInvoice;
using CreateInvoiceSystem.Modules.Invoices.BackgroundTasks;
using CreateInvoiceSystem.Modules.Invoices.Dto;
using CreateInvoiceSystem.Modules.Invoices.Entities;
using CreateInvoiceSystem.Modules.Invoices.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Mappers;
using MediatR;
using System.Threading.Channels;

namespace CreateInvoiceSystem.Modules.Invoices.Application.Handlers;

public class CreateInvoiceHandler(IInvoiceRepository _invoiceRepository, ChannelWriter<EmailTask> _writer)
    : IRequestHandler<CreateInvoiceRequest, CreateInvoiceResponse>
{
    public async Task<CreateInvoiceResponse> Handle(CreateInvoiceRequest request, CancellationToken cancellationToken)
    {        

        Client client = request.Invoice.ClientId is null
            ? await GetOrCreateClientAsync(request.Invoice, _invoiceRepository, cancellationToken)
            : await GetClientByIdAsync(request.Invoice.ClientId.Value, _invoiceRepository, cancellationToken);

        if (request.Invoice.ClientEmail is not null)
        {
            client.Email = request.Invoice.ClientEmail;
        }

        User user = await _invoiceRepository.GetUserByIdAsync(request.Invoice.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"User with ID {request.Invoice.UserId} not found.");

        Invoice entity = request.Invoice.ClientId is null
            ? InvoiceMappers.ToInvoiceWithNewClient(request.Invoice, client, user)
            : InvoiceMappers.ToInvoiceWithExistingClient(request.Invoice, client, user);

        await AddProductsToInvoicePositionsAsync(request.Invoice, entity, _invoiceRepository, cancellationToken);

        entity.RecalculateTotals();
        entity.Title = await GenerateInvoiceNumberAsync(request.Invoice.UserId, _invoiceRepository, cancellationToken);

        await _invoiceRepository.AddInvoiceAsync(entity, cancellationToken);

        var userEmail = await _invoiceRepository.GetUserEmailByIdAsync(request.Invoice.UserId, cancellationToken);

        if (!string.IsNullOrEmpty(userEmail))
        {
            await _writer.WriteAsync(new SellerEmailTask(userEmail, entity.Title), cancellationToken);
        }

        if (!string.IsNullOrWhiteSpace(entity.Client?.Email))
        {
            var dto = entity.ToDto();
            await _writer.WriteAsync(new ClientEmailTask(dto), cancellationToken);
        }

        return new CreateInvoiceResponse()
        {
            Data = entity.ToDto()
        };
    }

    private static async Task<string> GenerateInvoiceNumberAsync(int userId, IInvoiceRepository invoiceRepository, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        int maxNumber = await invoiceRepository.GetMaxInvoiceNumberInMonthAsync(userId, now.Month, now.Year, ct);
        int nextNumber = maxNumber + 1;

        return $"{nextNumber}/{now.Month:00}/{now.Year}";
    }

    private static async Task<Client> GetOrCreateClientAsync(CreateInvoiceDto param, IInvoiceRepository invoiceRepository, CancellationToken cancellationToken)
    {
        var client = await invoiceRepository.GetClientAsync(
            param.Client.Name,
            param.Client.Address.Street,
            param.Client.Address.Number,
            param.Client.Address.City,
            param.Client.Address.PostalCode,
            param.Client.Address.Country,
            param.UserId,
            param.ClientEmail ?? string.Empty,
            cancellationToken);

        if (client is not null)
            return client;

        var newClient = InvoiceMappers.ToEntity(param.Client);
        newClient.UserId = param.UserId;

        await invoiceRepository.AddClientAsync(newClient, cancellationToken);
        return newClient;
    }

    private static async Task<Client> GetClientByIdAsync(int clientId, IInvoiceRepository invoiceRepository, CancellationToken cancellationToken)
    {
        return await invoiceRepository.GetClientByIdAsync(clientId, cancellationToken)
            ?? throw new InvalidOperationException($"Client with ID {clientId} not found.");
    }

    private static async Task AddProductsToInvoicePositionsAsync(CreateInvoiceDto param, Invoice entity, IInvoiceRepository invoiceRepository, CancellationToken cancellationToken)
    {
        foreach (var position in param.InvoicePositions)
        {
            var product = position.ProductId is null
                ? await GetOrCreateProductAsync(position, param.UserId, invoiceRepository, cancellationToken)
                : await GetProductByIdAsync(position.ProductId.Value, invoiceRepository, cancellationToken);

            var invoicePosition = new InvoicePosition
            {
                Quantity = position.Quantity,
                Product = product,
                ProductId = product.ProductId > 0 ? product.ProductId : null,                
                ProductName = product.Name,
                ProductDescription = product.Description,
                ProductValue = product.Value,
                VatRate = position.VatRate
            };
            entity.InvoicePositions.Add(invoicePosition);
        }
    }

    private static async Task<Product> GetOrCreateProductAsync(InvoicePositionDto position, int userId, IInvoiceRepository invoiceRepository, CancellationToken cancellationToken)
    {
        var existing = await invoiceRepository.GetProductAsync(
            position.ProductName,
            position.ProductDescription,
            position.ProductValue,
            userId,
            cancellationToken);

        if (existing is not null) return existing;

        var newProduct = new Product
        {
            UserId = userId,
            Name = position.ProductName,
            Description = position.ProductDescription,
            Value = position.ProductValue
        };
        await invoiceRepository.AddProductAsync(newProduct, cancellationToken);
        return newProduct;
    }

    private static async Task<Product> GetProductByIdAsync(int productId, IInvoiceRepository invoiceRepository, CancellationToken cancellationToken)
    {
        return await invoiceRepository.GetProductByIdAsync(productId, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {productId} not found.");
    }
}