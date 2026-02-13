using Microsoft.AspNetCore.Components.Forms;

namespace CreateInvoiceSystem.Frontend.Components;

public class BootstrapValidationClassProvider : FieldCssClassProvider
{
    public override string GetFieldCssClass(EditContext editContext, in FieldIdentifier fieldIdentifier)
    {
        var hasErrors = editContext.GetValidationMessages(fieldIdentifier).Any();
        if (!editContext.IsModified(fieldIdentifier))
        {
            return string.Empty;
        }
        return hasErrors ? "is-invalid" : "is-valid";
    }
}
