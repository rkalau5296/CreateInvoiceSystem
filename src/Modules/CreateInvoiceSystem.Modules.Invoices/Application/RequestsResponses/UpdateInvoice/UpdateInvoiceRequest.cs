using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Invoices.Domain.Dto;
using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Invoices.Domain.Application.RequestsResponses.UpdateInvoice;

public record UpdateInvoiceRequest(int Id, UpdateInvoiceDto Invoice) : IRequest<UpdateInvoiceResponse>, ITransactionalRequest
{
    [JsonIgnore]
    public int UserId { get; set; }
}