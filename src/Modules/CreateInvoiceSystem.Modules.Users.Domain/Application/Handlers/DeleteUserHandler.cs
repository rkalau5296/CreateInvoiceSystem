using CreateInvoiceSystem.Abstractions.Notification;
using CreateInvoiceSystem.Modules.Users.Domain.Application.RequestsResponses.DeleteUser;
using CreateInvoiceSystem.Modules.Users.Domain.Interfaces;
using CreateInvoiceSystem.Modules.Users.Domain.Mappers;
using MediatR;

namespace CreateInvoiceSystem.Modules.Users.Domain.Application.Handlers;

public class DeleteUserHandler(IUserRepository _userRepository, IMediator mediator)
    : IRequestHandler<DeleteUserRequest, DeleteUserResponse>
{
    public async Task<DeleteUserResponse> Handle(DeleteUserRequest request, CancellationToken cancellationToken)
    {
        await mediator.Publish(new UserDeletedNotification(request.Id), cancellationToken);

        var user = await _userRepository.GetUserByIdAsync(request.Id, cancellationToken)
            ?? throw new InvalidOperationException($"User with ID {request.Id} not found.");

        if (user.Invoices.Any() || user.Clients.Any() || user.Products.Any())
        {
            throw new InvalidOperationException(
                $"Cannot delete User with ID {request.Id} because it has associated data.");
        }

        await _userRepository.RemoveAsync(request.Id, cancellationToken);

        if (user.Address is not null)
        {
            await _userRepository.RemoveAddress(user.AddressId, cancellationToken);
        }

        return new DeleteUserResponse
        {
            Data = UserMappers.ToDto(user)
        };
    }
}