using AskFlow.Infrastructure.Services;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Http;

namespace AskFlow.Tests.Infrastructure.Services
{
    public class SendGridSenderTests
    {
        private static SendGridClient BuildClient(HttpStatusCode statusCode, string body = "") =>
            new(new HttpClient(new FakeHttpHandler(statusCode, body)),
                new SendGridClientOptions { ApiKey = "fake-api-key" });

        private static SendGridMessage BuildMessage() =>
            new()
            {
                From = new EmailAddress("from@test.com", "Sender"),
                Subject = "Test"
            };

        [Fact]
        public async Task SendEmailAsync_WhenClientReturnsSuccess_ShouldReturnSuccessResponse()
        {
            var sender = new SendGridSender(BuildClient(HttpStatusCode.Accepted));

            var result = await sender.SendEmailAsync(BuildMessage(), CancellationToken.None);

            result.IsSuccessStatusCode.Should().BeTrue();
            result.StatusCode.Should().Be(HttpStatusCode.Accepted);
        }

        [Fact]
        public async Task SendEmailAsync_WhenClientReturnsError_ShouldReturnFailureResponse()
        {
            var sender = new SendGridSender(BuildClient(HttpStatusCode.Unauthorized, "{\"errors\":[]}"));

            var result = await sender.SendEmailAsync(BuildMessage(), CancellationToken.None);

            result.IsSuccessStatusCode.Should().BeFalse();
            result.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        private sealed class FakeHttpHandler(HttpStatusCode statusCode, string body = "") : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
                Task.FromResult(new HttpResponseMessage(statusCode) { Content = new StringContent(body) });
        }
    }
}
