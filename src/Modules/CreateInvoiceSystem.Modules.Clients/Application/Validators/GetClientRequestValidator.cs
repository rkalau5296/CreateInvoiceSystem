using CreateInvoiceSystem.Modules.Clients.Domain.Application.RequestsResponses.GetClient;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Clients.Domain.Application.Validators
{
    public class GetClientRequestValidator : AbstractValidator<GetClientRequest>
    {
        public GetClientRequestValidator()
        {
            RuleFor(x => x.Id).GreaterThanOrEqualTo(1)
                .WithMessage("Id must be greater than or equal to 1."); ;
        }
    }
}