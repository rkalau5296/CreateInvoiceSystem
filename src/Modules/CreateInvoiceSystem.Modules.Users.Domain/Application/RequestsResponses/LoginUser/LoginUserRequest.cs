using CreateInvoiceSystem.Modules.Users.Domain.Dto;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.LoginUser;

public record LoginUserRequest(LoginUserDto Dto) : IRequest<LoginUserResponse>;
