using Microsoft.JSInterop;

namespace Monolith.Core.Extensions;

public static class NotificationExtensions
{
    public const string NOTI_PLAYER_NAME = "notiPlayer";

    public static async Task PlayNoti(this IJSRuntime jsRuntime) =>
        await jsRuntime.InvokeVoidAsync("playAudio", NOTI_PLAYER_NAME);

    public static async Task PauseNoti(this IJSRuntime jsRuntime) =>
        await jsRuntime.InvokeVoidAsync("pauseAudio", NOTI_PLAYER_NAME);
}
