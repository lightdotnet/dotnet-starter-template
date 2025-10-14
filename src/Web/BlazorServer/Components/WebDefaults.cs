using Microsoft.FluentUI.AspNetCore.Components;

namespace Monolith.Components;

public abstract class WebDefaults
{
    public const string THEME_STORAGE_NAME = "theme";

    public static DialogParameters DefaultDialog(string title)
    {
        return new DialogParameters()
        {
            Title = title,
            PreventDismissOnOverlayClick = true,
            PreventScroll = true,
            Width = "400px",
        };
    }
}
