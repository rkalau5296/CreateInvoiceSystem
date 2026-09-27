using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClient;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Validators
{
    public class GetClientRequestValidator : AbstractValidator<GetClientRequest>
    {
        public GetClientRequestValidator()
        {
            RuleFor(x => x.Id).GreaterThanOrEqualTo(1);
        }
    }
}