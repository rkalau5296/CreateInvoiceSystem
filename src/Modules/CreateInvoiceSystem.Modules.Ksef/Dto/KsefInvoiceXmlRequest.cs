namespace CreateInvoiceSystem.Modules.Ksef.Dto;

public record KsefInvoiceXmlRequest(
    string SellerNip,
    string SellerName,
    string SellerAddress,
    string ClientNip,
    string ClientName,
    string ClientAddress,
    string ClientEmail,
    string Currency,
    string InvoiceNumber,
    DateTime IssueDate,
    DateTime PaymentDueDate,
    string MethodOfPayment,
    string BankAccountNumber,
    decimal TotalGross,
    List<KsefInvoiceLineXmlRequest> Lines);

public record KsefInvoiceLineXmlRequest(
    string ProductName,
    string UnitOfMeasure,
    decimal Quantity,
    decimal UnitPrice,
    decimal NetAmount,
    decimal VatAmount,
    string VatRate);