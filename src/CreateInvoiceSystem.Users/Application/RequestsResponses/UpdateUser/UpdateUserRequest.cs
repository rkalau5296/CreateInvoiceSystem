namespace CreateInvoiceSystem.Users.Application.RequestsResponses.UpdateUser;

using CreateInvoiceSystem.Abstractions.Dto;
using MediatR;

public class UpdateUserRequest(UpdateUserDto updateUser, int id) : IRequest<UpdateUserResponse>
{
    public UpdateUserDto User { get; } =
        (updateUser ?? throw new ArgumentNullException(nameof(updateUser),
            $"Argument '{nameof(updateUser)}' for user update request (Id={id}) cannot be null. Make sure the request body contains all required fields."
        )) with { UserId = id };

    public int Id { get; } =
        id >= 1 ? id
            : throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than or equal to 1.");
}
