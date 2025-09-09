using Microsoft.AspNetCore.Components;

namespace Monolith.WebAdmin;

public static class NavigationManagerExtensions
{
    private static string GetPathAndQuery(NavigationManager navigationManager)
    {
        var absoluteUri = navigationManager.ToAbsoluteUri(navigationManager.Uri);

        // pathAndQuery will start with "/", get from char at number 1 for remove that
        return absoluteUri.PathAndQuery[1..];
    }

    public static void RedirectToLogin(this NavigationManager navigationManager)
    {
        var loginPath = "account/login";

        var pathAndQuery = GetPathAndQuery(navigationManager);

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

    public static void RedirectToLogout(this NavigationManager navigationManager)
    {
        var logoutPath = "account/logout";

        var pathAndQuery = GetPathAndQuery(navigationManager);

        if (!string.IsNullOrEmpty(pathAndQuery))
        {
            var returnUrl = Uri.EscapeDataString(pathAndQuery);

            logoutPath += $"?returnUrl={returnUrl}";
        }

        navigationManager.NavigateTo(logoutPath, true);
    }
}
