using Light.Blazor;

namespace Light.FluentBlazor;

public interface ICallGuardedService : ICallGuarded
{
}

public class CallGuardedService(
    IToastDisplay toast,
    IDialogDisplay dialog,
    SpinnerService spinner) : CallGuarded(toast, dialog, spinner), ICallGuardedService
{
}
