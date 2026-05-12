using AskFlow.Application.Interfaces;
using AskFlow.Infrastructure.Settings;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace AskFlow.Infrastructure.Services
{
    public class AzureBlobAvatarStorage : IAvatarStorage
    {
        private readonly BlobContainerClient _container;

        public AzureBlobAvatarStorage(IOptions<BlobStorageSettings> options)
            : this(new BlobContainerClient(options.Value.ConnectionString, options.Value.AvatarContainer))
        {
        }

        internal AzureBlobAvatarStorage(BlobContainerClient container)
        {
            _container = container;
        }

        public async Task<string> UploadAsync(Stream content, string contentType, string userId, CancellationToken cancellationToken)
        {
            var blob = _container.GetBlobClient(BuildBlobName(userId, contentType));

            await blob.UploadAsync(content, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType,
                    CacheControl = "public, max-age=31536000"
                }
            }, cancellationToken);

            return blob.Uri.ToString();
        }

        public async Task DeleteAsync(string userId, CancellationToken cancellationToken)
        {
            await foreach (var blob in _container.GetBlobsAsync(BlobTraits.None, BlobStates.None, $"{userId}.", cancellationToken))
                await _container.DeleteBlobIfExistsAsync(blob.Name, cancellationToken: cancellationToken);
        }

        private static string BuildBlobName(string userId, string contentType) => contentType switch
        {
            "image/png" => $"{userId}.png",
            "image/webp" => $"{userId}.webp",
            _ => $"{userId}.jpg"
        };
    }
}
