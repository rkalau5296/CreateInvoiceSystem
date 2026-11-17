using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetIncomingKsefInvoices;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceDetails;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoicePdf;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceStatus;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.GetKsefInvoiceXml;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.KsefSettings;
using CreateInvoiceSystem.Modules.Ksef.Application.RequestsResponses.SendInvoiceToKsef;
using FluentValidation;

namespace CreateInvoiceSystem.Modules.Ksef.Application.Validators;

public sealed class GetIncomingKsefInvoicesRequestValidator : AbstractValidator<GetIncomingKsefInvoicesRequest>
{
    public GetIncomingKsefInvoicesRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.FromDate).LessThanOrEqualTo(x => x.ToDate)
            .WithMessage("Data początkowa nie może być późniejsza niż data końcowa.");
        RuleFor(x => x.PageSize).InclusiveBetween(1, 100)
            .WithMessage("Rozmiar strony musi wynosić od 1 do 100.");
        RuleFor(x => x.PageNumber).Cascade(CascadeMode.Stop)
            .GreaterThan(0).WithMessage("Nieprawidłowy numer strony.")
            .Must((request, page) => ((long)page - 1) * request.PageSize <= int.MaxValue)
            .When(x => x.PageSize is >= 1 and <= 100, ApplyConditionTo.CurrentValidator)
            .WithMessage("Nieprawidłowy numer strony.");
    }
}

public sealed class GetKsefInvoiceDetailsRequestValidator : AbstractValidator<GetKsefInvoiceDetailsRequest>
{
    public GetKsefInvoiceDetailsRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.KsefNumber).NotEmpty().WithMessage("Podaj numer KSeF.");
    }
}

public sealed class GetKsefInvoicePdfRequestValidator : AbstractValidator<GetKsefInvoicePdfRequest>
{
    public GetKsefInvoicePdfRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.KsefNumber).NotEmpty().WithMessage("Podaj numer KSeF.");
    }
}

public sealed class GetKsefInvoiceXmlRequestValidator : AbstractValidator<GetKsefInvoiceXmlRequest>
{
    public GetKsefInvoiceXmlRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.KsefNumber).NotEmpty().WithMessage("Podaj numer KSeF.");
    }
}

public sealed class GetKsefInvoiceStatusRequestValidator : AbstractValidator<GetKsefInvoiceStatusRequest>
{
    public GetKsefInvoiceStatusRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.InvoiceId).GreaterThan(0).WithMessage("Identyfikator faktury musi być większy od zera.");
    }
}

public sealed class SendInvoiceToKsefRequestValidator : AbstractValidator<SendInvoiceToKsefRequest>
{
    public SendInvoiceToKsefRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.InvoiceId).GreaterThan(0).WithMessage("Identyfikator faktury musi być większy od zera.");
    }
}

public sealed class GetKsefSettingsRequestValidator : AbstractValidator<GetKsefSettingsRequest>
{
    public GetKsefSettingsRequestValidator() => RuleFor(x => x.UserId).GreaterThan(0);
}

public sealed class DeleteKsefSettingsRequestValidator : AbstractValidator<DeleteKsefSettingsRequest>
{
    public DeleteKsefSettingsRequestValidator() => RuleFor(x => x.UserId).GreaterThan(0);
}

public sealed class SaveKsefSettingsRequestValidator : AbstractValidator<SaveKsefSettingsRequest>
{
    public SaveKsefSettingsRequestValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Token)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Podaj token KSeF.")
            .MaximumLength(4096).WithMessage("Token KSeF może mieć maksymalnie 4096 znaków.")
            .Must(token => !token.Any(char.IsWhiteSpace))
            .WithMessage("Podaj poprawny token KSeF (bez spacji, maksymalnie 4096 znaków).");
    }
}
