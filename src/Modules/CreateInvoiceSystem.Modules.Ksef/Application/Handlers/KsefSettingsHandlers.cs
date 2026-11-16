using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.KsefSettings;
using CreateInvoiceSystem.Modules.Ksef.Interfaces;
using MediatR;

namespace CreateInvoiceSystem.Modules.Ksef.Application.Handlers;

public sealed class GetKsefSettingsHandler(IKsefCredentialsStore credentialsStore)
    : IRequestHandler<GetKsefSettingsRequest, bool>
{
    public Task<bool> Handle(GetKsefSettingsRequest request, CancellationToken cancellationToken) =>
        credentialsStore.IsConfiguredAsync(request.UserId, cancellationToken);
}

public sealed class SaveKsefSettingsHandler(IKsefCredentialsStore credentialsStore)
    : IRequestHandler<SaveKsefSettingsRequest>
{
    public Task Handle(SaveKsefSettingsRequest request, CancellationToken cancellationToken) =>
        credentialsStore.SaveAsync(request.UserId, request.Token, cancellationToken);
}

public sealed class DeleteKsefSettingsHandler(IKsefCredentialsStore credentialsStore)
    : IRequestHandler<DeleteKsefSettingsRequest>
{
    public Task Handle(DeleteKsefSettingsRequest request, CancellationToken cancellationToken) =>
        credentialsStore.DeleteAsync(request.UserId, cancellationToken);
}
