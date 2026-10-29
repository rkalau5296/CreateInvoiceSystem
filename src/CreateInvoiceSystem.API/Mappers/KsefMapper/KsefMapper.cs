using CreateInvoiceSystem.Modules.Invoices.Entities;
using CreateInvoiceSystem.Modules.Ksef.Dto;

namespace CreateInvoiceSystem.API.Mappers.KsefMapper;

public static class KsefMapper
{
    public static KsefInvoiceXmlRequest ToKsefInvoiceXmlRequest(this Invoice invoice)
    {
        return new KsefInvoiceXmlRequest(
            SellerNip: invoice.SellerNip,
            SellerName: invoice.SellerName,
            SellerAddress: invoice.SellerAddress,
            ClientNip: invoice.ClientNip,
            ClientName: invoice.ClientName,
            ClientAddress: invoice.ClientAddress,
            ClientEmail: invoice.ClientEmail,
            Currency: invoice.Currency,
            InvoiceNumber: invoice.Title,
            IssueDate: invoice.CreatedDate,
            PaymentDueDate: invoice.PaymentDate,
            MethodOfPayment: invoice.MethodOfPayment,
            BankAccountNumber: invoice.BankAccountNumber,
            TotalGross: invoice.TotalGross,
            Lines: invoice.InvoicePositions.Select(p => new KsefInvoiceLineXmlRequest(
                ProductName: p.ProductName,
                UnitOfMeasure: p.UnitOfMeasure,
                Quantity: p.Quantity,
                UnitPrice: p.ProductValue ?? 0,
                NetAmount: p.GetNetValue(),
                VatAmount: p.GetVatValue(),
                VatRate: p.VatRate)).ToList());
    }
}