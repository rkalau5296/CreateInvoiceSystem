using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreateInvoiceSystem.Modules.Clients.Services
{
    public interface IInvoiceReadService
    {
        Task<bool> IsClientUsedAsync(int productId, CancellationToken cancellationToken = default);
    }
}
