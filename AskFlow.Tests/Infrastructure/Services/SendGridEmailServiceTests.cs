using AskFlow.Infrastructure.Services;
using AskFlow.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Http;

namespace AskFlow.Tests.Infrastructure.Services
{
    public class SendGridEmailServiceTests
    {
        private readonly ISendGridSender _sender = Substitute.For<ISendGridSender>();
        private readonly ILogger<SendGridEmailService> _logger = Substitute.For<ILogger<SendGridEmailService>>();

        private static SendGridSettings DefaultSettings() => new()
        {
            ApiKey = "SG.test-key",
            FromAddress = "no-reply@askflow.com",
            FromName = "AskFlow",
            FrontendUrl = "https://askflow.com"
        };

        private SendGridEmailService CreateSut(SendGridSettings? settings = null) =>
            new(_sender, settings ?? DefaultSettings(), _logger);

        private void SetupResponse(HttpStatusCode statusCode, string body = "") =>
            _sender.SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>())
                   .Returns(new Response(statusCode, new StringContent(body), null!));

        [Fact]
        public void Constructor_WithOptions_ShouldNotThrow()
        {
            var act = () => new SendGridEmailService(Options.Create(DefaultSettings()), _logger);
            act.Should().NotThrow();
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_WhenResponseSucceeds_ShouldComplete()
        {
            SetupResponse(HttpStatusCode.Accepted);

            var act = () => CreateSut().SendPasswordResetEmailAsync("user@test.com", "token", default);

            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_WhenResponseFails_ShouldThrow()
        {
            SetupResponse(HttpStatusCode.BadRequest, "{\"errors\":[]}");

            var act = () => CreateSut().SendPasswordResetEmailAsync("user@test.com", "token", default);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldCallSenderOnce()
        {
            SetupResponse(HttpStatusCode.Accepted);

            await CreateSut().SendPasswordResetEmailAsync("user@test.com", "token", default);

            await _sender.Received(1).SendEmailAsync(Arg.Any<SendGridMessage>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldSendToCorrectRecipient()
        {
            SetupResponse(HttpStatusCode.Accepted);

            await CreateSut().SendPasswordResetEmailAsync("user@test.com", "token", default);

            await _sender.Received(1).SendEmailAsync(
                Arg.Is<SendGridMessage>(m => m.Personalizations[0].Tos.Any(t => t.Email == "user@test.com")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldSetFromAddressAndName()
        {
            SetupResponse(HttpStatusCode.Accepted);
            var settings = new SendGridSettings
            {
                ApiKey = "SG.test-key",
                FromAddress = "noreply@company.com",
                FromName = "Company",
                FrontendUrl = "https://askflow.com"
            };

            await CreateSut(settings).SendPasswordResetEmailAsync("user@test.com", "token", default);

            await _sender.Received(1).SendEmailAsync(
                Arg.Is<SendGridMessage>(m => m.From.Email == "noreply@company.com" && m.From.Name == "Company"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldIncludeResetUrlInHtmlContent()
        {
            SetupResponse(HttpStatusCode.Accepted);
            var settings = new SendGridSettings
            {
                ApiKey = "SG.test-key",
                FromAddress = "no-reply@askflow.com",
                FromName = "AskFlow",
                FrontendUrl = "https://myapp.com"
            };

            await CreateSut(settings).SendPasswordResetEmailAsync("user@test.com", "abc123", default);

            await _sender.Received(1).SendEmailAsync(
                Arg.Is<SendGridMessage>(m =>
                    m.Contents != null &&
                    m.Contents.Any(c => c.Value.Contains("https://myapp.com/reset-password?token=abc123"))),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldUrlEncodeTokenAndEmail()
        {
            SetupResponse(HttpStatusCode.Accepted);

            await CreateSut().SendPasswordResetEmailAsync("user+a@test.com", "tok+en/==", default);

            await _sender.Received(1).SendEmailAsync(
                Arg.Is<SendGridMessage>(m =>
                    m.Contents != null &&
                    m.Contents.Any(c =>
                        c.Value.Contains("tok%2Ben%2F%3D%3D") &&
                        c.Value.Contains("user%2Ba%40test.com"))),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldDisableClickTracking()
        {
            SetupResponse(HttpStatusCode.Accepted);

            await CreateSut().SendPasswordResetEmailAsync("user@test.com", "token", default);

            await _sender.Received(1).SendEmailAsync(
                Arg.Is<SendGridMessage>(m =>
                    m.TrackingSettings != null &&
                    m.TrackingSettings.ClickTracking != null &&
                    m.TrackingSettings.ClickTracking.Enable == false),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task SendPasswordResetEmailAsync_ShouldPropagateCancellationToken()
        {
            SetupResponse(HttpStatusCode.Accepted);
            using var cts = new CancellationTokenSource();

            await CreateSut().SendPasswordResetEmailAsync("user@test.com", "token", cts.Token);

            await _sender.Received(1).SendEmailAsync(Arg.Any<SendGridMessage>(), cts.Token);
        }
    }
}
