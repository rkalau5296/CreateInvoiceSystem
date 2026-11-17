using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Dto;
using MediatR;
using System.Text.Json.Serialization;

namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.UpdateUser;

public class UpdateUserRequest(UpdateUserDto user, int id) : IRequest<UpdateUserResponse>, ITransactionalRequest
{
    public UpdateUserDto User { get; } = user;
    public int Id { get; } = id;

    [JsonIgnore]
    public int UserId { get; set; } = id;
}