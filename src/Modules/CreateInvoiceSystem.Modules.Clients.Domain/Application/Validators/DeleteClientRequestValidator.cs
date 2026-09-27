using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.DeleteClient;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Validators
{
    public class DeleteClientRequestValidator : AbstractValidator<DeleteClientRequest>
    {
        public DeleteClientRequestValidator()
        {
            RuleFor(x => x.Id).GreaterThanOrEqualTo(1);
        }
    }
}