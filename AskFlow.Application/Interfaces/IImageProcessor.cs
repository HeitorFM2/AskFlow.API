namespace AskFlow.Application.Interfaces
{
    public interface IImageProcessor
    {
        Task<ProcessedImage?> ProcessAvatarAsync(Stream content, CancellationToken cancellationToken);
    }

    public sealed record ProcessedImage(Stream Content, string ContentType) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync() => await Content.DisposeAsync();
    }
}
