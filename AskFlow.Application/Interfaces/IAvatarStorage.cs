namespace AskFlow.Application.Interfaces
{
    public interface IAvatarStorage
    {
        Task<string> UploadAsync(Stream content, string contentType, string userId, CancellationToken cancellationToken);
        Task DeleteAsync(string userId, CancellationToken cancellationToken);
    }
}
