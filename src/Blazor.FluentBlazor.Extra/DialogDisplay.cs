using Light.Blazor;
using Microsoft.FluentUI.AspNetCore.Components;

namespace Light.FluentBlazor;

internal class DialogDisplay(IDialogService dialogService) : IDialogDisplay
{
    public async Task<bool> ShowWarning(string message)
    {
        var res = await dialogService.ShowWarningAsync(message);

        return !res.Result.IsCanceled;
    }
}
