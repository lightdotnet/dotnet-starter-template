using MudBlazor;

namespace Monolith.Blazor.Shared;

public class WebConstants
{
    public const string APP_NAME = "Blazor UI beta";

    public const string COMPANY_NAME = "LightsoftHub";

    public const string LoginPath = "/account/login";

    public const string LogoutPath = "/account/logout";

    public const int ItemsPerPage = 20;

    public static DialogOptions DEFAULT_DIALOG_OPTIONS => new()
    {
        BackdropClick = false,
        CloseOnEscapeKey = false,
        CloseButton = true,
        FullWidth = true,
        MaxWidth = MaxWidth.ExtraSmall,
    };

    public static DialogOptions LARGE_DIALOG_OPTIONS => new()
    {
        BackdropClick = false,
        CloseOnEscapeKey = false,
        CloseButton = true,
        FullWidth = true,
        MaxWidth = MaxWidth.Medium,
    };
}
