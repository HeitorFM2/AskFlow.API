namespace AskFlow.Infrastructure.Settings
{
    public class BlobStorageSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string AvatarContainer { get; set; } = "avatars";
    }
}
