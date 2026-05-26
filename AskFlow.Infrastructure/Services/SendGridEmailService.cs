using AskFlow.Application.Interfaces;
using AskFlow.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;

namespace AskFlow.Infrastructure.Services
{
    internal interface ISendGridSender
    {
        Task<Response> SendEmailAsync(SendGridMessage message, CancellationToken cancellationToken);
    }

    internal sealed class SendGridSender(SendGridClient client) : ISendGridSender
    {
        public Task<Response> SendEmailAsync(SendGridMessage message, CancellationToken cancellationToken)
            => client.SendEmailAsync(message, cancellationToken);
    }

    public class SendGridEmailService : IEmailService
    {
        private readonly ISendGridSender _sender;
        private readonly SendGridSettings _settings;
        private readonly ILogger<SendGridEmailService> _logger;

        public SendGridEmailService(IOptions<SendGridSettings> settings, ILogger<SendGridEmailService> logger)
            : this(new SendGridSender(new SendGridClient(settings.Value.ApiKey)), settings.Value, logger) { }

        internal SendGridEmailService(ISendGridSender sender, SendGridSettings settings, ILogger<SendGridEmailService> logger)
        {
            _sender = sender;
            _settings = settings;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken)
        {
            var encodedToken = WebUtility.UrlEncode(resetToken);
            var resetUrl = $"{_settings.FrontendUrl}/reset-password?token={encodedToken}&email={WebUtility.UrlEncode(email)}";

            var htmlBody = $"""
                    <div style="font-family:sans-serif;max-width:480px;margin:0 auto;padding:32px;">
                        <h2 style="color:#1e2235;margin-bottom:8px;">Reset your password</h2>
                        <p style="color:#555;margin-bottom:24px;">
                            Click the button below to reset your password. This link expires in 1 hour.
                        </p>
                        <a href="{resetUrl}"
                           style="display:inline-block;background-color:#1e2235;color:#fff;
                                  text-decoration:none;padding:12px 28px;border-radius:6px;
                                  font-weight:bold;font-size:15px;">
                            Reset Password
                        </a>
                        <p style="color:#999;font-size:12px;margin-top:24px;">
                            If you didn't request this, you can safely ignore this email.
                        </p>
                    </div>
                    """;

            var message = new SendGridMessage
            {
                From = new EmailAddress(_settings.FromAddress, _settings.FromName),
                Subject = "Password Reset Request",
                TrackingSettings = new TrackingSettings
                {
                    ClickTracking = new ClickTracking { Enable = false }
                }
            };
            message.AddContent(MimeType.Html, htmlBody);
            message.AddTo(new EmailAddress(email));

            var response = await _sender.SendEmailAsync(message, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Body.ReadAsStringAsync(cancellationToken);
                _logger.LogError("Failed to send password reset email to {Email}. Status: {Status}. Body: {Body}",
                    email, response.StatusCode, body);

                throw new InvalidOperationException("Failed to send password reset email.");
            }

            _logger.LogInformation("Password reset email sent to {Email}.", email);
        }
    }
}
