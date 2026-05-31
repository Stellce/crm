using System.Net;
using System.Net.Mail;
using Application.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Email;

public sealed class SmtpEmailSender(
    IOptions<SmtpEmailOptions> options,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly SmtpEmailOptions _options = options.Value;

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            logger.LogInformation(
                "Email sending is disabled. To: {To}, Subject: {Subject}, Body: {Body}",
                to,
                subject,
                body);

            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.From),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(new MailAddress(to));

        using var smtpClient = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.EnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_options.User) &&
            !string.IsNullOrWhiteSpace(_options.Password))
        {
            smtpClient.Credentials = new NetworkCredential(
                _options.User,
                _options.Password);
        }

        await smtpClient.SendMailAsync(message, cancellationToken);
    }
}