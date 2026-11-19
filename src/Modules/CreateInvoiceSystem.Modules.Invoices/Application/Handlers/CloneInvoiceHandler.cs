using CreateInvoiceSystem.Modules.Invoices.Application.RequestsResponses.CloneInvoice;
using CreateInvoiceSystem.Modules.Invoices.Entities;
using CreateInvoiceSystem.Modules.Invoices.Interfaces;
using CreateInvoiceSystem.Modules.Invoices.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Invoices.Application.Handlers;

public class CloneInvoiceHandler(IInvoiceRepository _invoiceRepository, TimeProvider? timeProvider = null)
    : IRequestHandler<CloneInvoiceRequest, CloneInvoiceResponse>
{
    private const int DefaultPaymentTermDays = 14;
    private readonly TimeProvider _clock = timeProvider ?? TimeProvider.System;

    public async Task<CloneInvoiceResponse> Handle(CloneInvoiceRequest request, CancellationToken cancellationToken)
    {
        var source = await _invoiceRepository.GetInvoiceByIdAsync(request.UserId, request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"Invoice with ID {request.Id} not found.");

        var user = await _invoiceRepository.GetUserByIdAsync(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"User with ID {request.UserId} not found.");

        var today = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(
            _clock.GetUtcNow(), "Central European Standard Time").Date;

        var paymentTermDays = (source.PaymentDate.Date - source.CreatedDate.Date).Days;
        if (paymentTermDays < 0)
            paymentTermDays = DefaultPaymentTermDays;

        var entity = new Invoice
        {
            Title = await InvoiceNumbering.GenerateNextAsync(request.UserId, today, _invoiceRepository, cancellationToken),
            CreatedDate = today,
            PaymentDate = today.AddDays(paymentTermDays),
            Comments = source.Comments,
            UserId = request.UserId,
            MethodOfPayment = source.MethodOfPayment,
            Currency = source.Currency,
            SellerName = user.CompanyName,
            SellerNip = user.Nip,
            SellerAddress = InvoiceMappers.FormatAddress(user.Address),
            BankAccountNumber = user.BankAccountNumber,
            ClientName = source.ClientName,
            ClientNip = source.ClientNip,
            ClientAddress = source.ClientAddress,
            ClientEmail = source.ClientEmail,
            InvoicePositions = [.. source.InvoicePositions.Select(p => new InvoicePosition
            {
                ProductName = p.ProductName,
                ProductDescription = p.ProductDescription,
                ProductValue = p.ProductValue,
                Quantity = p.Quantity,
                UnitOfMeasure = p.UnitOfMeasure,
                VatRate = p.VatRate
            })]
        };

        entity.RecalculateTotals();

        await _invoiceRepository.AddInvoiceAsync(entity, cancellationToken);

        return new CloneInvoiceResponse { Data = entity.ToDto() };
    }
}
