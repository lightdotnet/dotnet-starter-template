using Microsoft.AspNetCore.Components;

namespace Monolith.Core.Extensions;

public static class NavigationManagerExtensions
{
    public static void RedirectToLogin(this NavigationManager navigationManager)
    {
        var loginPath = "account/login";

        var absoluteUri = navigationManager.ToAbsoluteUri(navigationManager.Uri);

        // pathAndQuery will start with "/", get from char at number 1 for remove that
        var pathAndQuery = absoluteUri.PathAndQuery[1..];

        if (pathAndQuery.StartsWith(loginPath))
        {
            return;
        }

        if (!string.IsNullOrEmpty(pathAndQuery))
        {
            var returnUrl = Uri.EscapeDataString(pathAndQuery);

            loginPath += $"?returnUrl={returnUrl}";
        }

        navigationManager.NavigateTo(loginPath);
    }

    public static void RedirectWhenLoginSuccess(this NavigationManager navigationManager, string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl))
        {
            navigationManager.NavigateTo(returnUrl);
            return;
        }

        var currentPath = navigationManager.ToBaseRelativePath(navigationManager.Uri);

        if (currentPath.StartsWith("account/login"))
        {
            navigationManager.NavigateTo("/");
        }
        else
        {
            navigationManager.NavigateTo(currentPath);
        }
    }
}
