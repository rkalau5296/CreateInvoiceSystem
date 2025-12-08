using CreateInvoiceSystem.Modules.Clients.Dto;
using CreateInvoiceSystem.Modules.Clients.Entities;
using CreateInvoiceSystem.Modules.Clients.Mappers;
using CreateInvoiceSystem.Modules.Clients.Persistence;
using Microsoft.Extensions.Logging;

namespace CreateInvoiceSystem.Modules.Clients.Application.Commands;

public class CreateClientCommand
{
    private IClientDbContext _dbContext;
    private ILogger<CreateClientCommand> _logger;

    public CreateClientCommand(
        IClientDbContext dbContext, 
        ILogger<CreateClientCommand> logger
    )
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ClientDto> Execute(
        CreateClientDto parametr,
        CancellationToken cancellationToken = default
    )
    {
        if (parametr is null)
            throw new ArgumentNullException(nameof(parametr));

        var entity = parametr.ToEntity();

        await _dbContext.Clients.AddAsync(entity, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return entity.ToDto();
    }
}
