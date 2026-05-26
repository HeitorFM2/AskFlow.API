namespace AskFlow.Infrastructure.Settings
{
    public class SendGridSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string FromAddress { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string FrontendUrl { get; set; } = string.Empty;
    }
}
