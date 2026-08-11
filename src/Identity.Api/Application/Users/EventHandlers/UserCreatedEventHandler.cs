using Light.Mediator;
using StarterKit.Identity.Api.Events;
using StarterKit.Notifications.Contracts.Services;

namespace StarterKit.Identity.Api.Application.Users.EventHandlers;

internal class UserCreatedEventHandler(
    IMailService mailService,
    ILogger<UserCreatedEventHandler> logger)
    : INotificationHandler<UserCreatedEvent>
{
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("User created: {UserId}, {UserName}, {Email}",
            notification.UserId,
            notification.UserName,
            notification.Email);

        // Additional logic can be added here, such as sending a welcome email or logging to an external service.

        if (!string.IsNullOrEmpty(notification.Email))
        {
            try
            {
                await mailService.SendFromSystemAsync(
                    [notification.Email],
                    "Welcome to system!",
                    GenerateWelcomeEmailBody(notification.UserName ?? notification.Email),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send welcome email to {Email}", notification.Email);
            }
        }
    }

    private static string GenerateWelcomeEmailBody(string user)
    {
        var welcomeEmail = "" +
            $"Dear {user}," +
            "<br><br>" +
            "We are pleased to provide you with your login credentials for our system. Please find the details below:" +
            "<br>" +
            $"- <b>Username</b>: {user}" +
            "<br>" +
            $"- <b>Password</b>: ***is_your_company_account_p@ssword***" +
            "<br><br>" +
            "If you have any questions or need further assistance, feel free to reach out." +
            "<br><br>" +
            "Warm regards!";

        return welcomeEmail;
    }
}
