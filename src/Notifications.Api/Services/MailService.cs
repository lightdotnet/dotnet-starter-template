using Light.Mail;
using Light.SmtpMail;
using StarterKit.Notifications.Contracts.Services;

namespace StarterKit.Notifications.Api.Services;

internal class MailService(
    ISmtpMailSender smtpMailSender,
    SmtpMailKitOptions options) : IMailService
{
    public Task SendFromSystemAsync(
        List<string> recipients,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        // fake system email address for now, we can change it later
        var systemEmail = options.UserName;

        return smtpMailSender.SendAsync(
            new MailFrom(systemEmail, "System"),
            new MailMessage
            {
                Recipients = recipients,
                Subject = subject,
                Content = body,
            },
            cancellationToken);
    }

    public Task SendAsync(
        string from,
        List<string> recipients,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        return smtpMailSender.SendAsync(
            new MailFrom(from),
            new MailMessage
            {
                Recipients = recipients,
                Subject = subject,
                Content = body,
            },
            cancellationToken);
    }
}
