using MediatR;

namespace CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.KsefSettings;

public sealed class GetKsefSettingsRequest(int userId) : IRequest<bool>
{
    public int UserId { get; } = userId;
}

public sealed class SaveKsefSettingsRequest(int userId, string token) : IRequest
{
    public int UserId { get; } = userId;
    public string Token { get; } = token;
}

public sealed class DeleteKsefSettingsRequest(int userId) : IRequest
{
    public int UserId { get; } = userId;
}
