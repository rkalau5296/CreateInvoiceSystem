using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Domain.Dto;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.GetUsers;
public class GetUsersResponse : ResponseBase<List<UserDto>>
{    
}