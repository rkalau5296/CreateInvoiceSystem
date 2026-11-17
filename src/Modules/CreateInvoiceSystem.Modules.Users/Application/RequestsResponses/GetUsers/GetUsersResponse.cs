using CreateInvoiceSystem.Abstractions.CQRS;
using CreateInvoiceSystem.Modules.Users.Dto;

namespace CreateInvoiceSystem.Modules.Users.Application.RequestsResponses.GetUsers;
public class GetUsersResponse : ResponseBase<List<UserDto>>
{    
}