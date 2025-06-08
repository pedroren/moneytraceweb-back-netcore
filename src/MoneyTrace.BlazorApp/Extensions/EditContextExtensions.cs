using ErrorOr;
using Microsoft.AspNetCore.Components.Forms;

namespace MoneyTrace.BlazorApp.Extensions;

/// <summary>
/// Custom EditContext extension to handle ErrorOr validation
/// </summary>
public static class EditContextExtensions
{
    public static void AddValidationErrors(this EditContext editContext, List<Error> errors)
    {
        var validationMessageStore = new ValidationMessageStore(editContext);

        foreach (var error in errors)
        {
            // If the error has a property path, use it; otherwise, add to model level
            if (!string.IsNullOrEmpty(error.Code) && IsPropertyName(error.Code))
            {
                var fieldIdentifier = editContext.Field(error.Code);
                validationMessageStore.Add(fieldIdentifier, error.Description);
            }
            else
            {
                // Add as model-level error
                var fieldIdentifier = new FieldIdentifier(editContext.Model, string.Empty);
                validationMessageStore.Add(fieldIdentifier, error.Description);
            }
        }

        editContext.NotifyValidationStateChanged();
    }

    public static void ClearValidationErrors(this EditContext editContext)
    {
        var validationMessageStore = new ValidationMessageStore(editContext);
        validationMessageStore.Clear();
        editContext.NotifyValidationStateChanged();
    }

    private static bool IsPropertyName(string code)
    {
        // Simple heuristic - you might want to make this more sophisticated
        return char.IsUpper(code[0]) && !code.Contains(' ');
    }
}