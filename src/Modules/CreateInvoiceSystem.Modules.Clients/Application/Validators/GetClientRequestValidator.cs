using CreateInvoiceSystem.Modules.Clients.Application.RequestsResponses.GetClient;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Clients.Application.Validators
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