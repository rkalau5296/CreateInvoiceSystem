namespace CreateInvoiceSystem.Products.Application.Validators;

using CreateInvoiceSystem.Products.Application.RequestsResponses.CreateProduct;
using FluentValidation;


public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Product.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100);
        
        //RuleFor(x => x.UserId)
        //    .GreaterThan(0).WithMessage("UserId is required.");
                
    }
}