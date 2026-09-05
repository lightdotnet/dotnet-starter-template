using StarterKit.Notifications.Api.SignalR;
using StarterKit.Notifications.Contracts.Services;
using StarterKit.Notifications.Contracts.SystemNotifications;

namespace StarterKit.Notifications.Api.Application.Notifications.Commands;

internal sealed record SendNotificationCommand(
    string FromUserId,
    string? FromName,
    string ToUserId,
    SystemMessage Message) : ICommand<IResult>;

internal class SendNotificationCommandHandler(
    INotificationService notificationService,
    IHubService hub)
    : ICommandHandler<SendNotificationCommand, IResult>
{
    public async Task<IResult> Handle(
        SendNotificationCommand request,
        CancellationToken cancellationToken)
    {
        await notificationService.SaveAsync(
            request.FromUserId, request.FromName, request.ToUserId, request.Message);

        // Push after the record is saved so a client reacting to the push can load
        // the persisted entry from the API. The payload itself is sent to the client too.
        await hub.SendAsync(request.Message, request.ToUserId, cancellationToken);

        return Result.Success();
    }
}
