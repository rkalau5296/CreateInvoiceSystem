using CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.DeleteClient;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Clients.Application.Validators
{
    public class DeleteClientRequestValidator : AbstractValidator<DeleteClientRequest>
    {
        public DeleteClientRequestValidator()
        {
            RuleFor(x => x.Id).GreaterThanOrEqualTo(1)
                .WithMessage("Id must be greater than or equal to 1."); ;
        }
    }
}