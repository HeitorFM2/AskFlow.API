using AskFlow.Infrastructure.Services;
using AskFlow.Infrastructure.Settings;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace AskFlow.Tests.Infrastructure.Services
{
    public class AzureBlobAvatarStorageTests
    {
        private readonly BlobContainerClient _container = Substitute.For<BlobContainerClient>();

        private AzureBlobAvatarStorage CreateSut() => new(_container);

        [Fact]
        public void Constructor_WithOptions_ShouldBuildFromSettings_WithoutThrowing()
        {
            var settings = new BlobStorageSettings
            {
                ConnectionString = "UseDevelopmentStorage=true",
                AvatarContainer = "custom-avatars"
            };

            var act = () => new AzureBlobAvatarStorage(Options.Create(settings));

            act.Should().NotThrow();
        }

        private BlobClient SetupBlobClient(string expectedName, string uri = "https://acc.blob.core.windows.net/avatars/x")
        {
            var blob = Substitute.For<BlobClient>();
            blob.Uri.Returns(new Uri(uri));
            blob.UploadAsync(Arg.Any<Stream>(), Arg.Any<BlobUploadOptions>(), Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Substitute.For<Response<BlobContentInfo>>()));
            _container.GetBlobClient(expectedName).Returns(blob);
            return blob;
        }

        [Theory]
        [InlineData("image/png", "u1.png")]
        [InlineData("image/webp", "u1.webp")]
        [InlineData("image/jpeg", "u1.jpg")]
        [InlineData("application/octet-stream", "u1.jpg")]
        public async Task UploadAsync_ShouldPickBlobName_BasedOnContentType(string contentType, string expectedName)
        {
            SetupBlobClient(expectedName);
            using var stream = new MemoryStream();

            await CreateSut().UploadAsync(stream, contentType, "u1", CancellationToken.None);

            _container.Received(1).GetBlobClient(expectedName);
        }

        [Fact]
        public async Task UploadAsync_ShouldReturn_BlobUriAsString()
        {
            SetupBlobClient("u1.png", uri: "https://acc.blob.core.windows.net/avatars/u1.png");
            using var stream = new MemoryStream();

            var result = await CreateSut().UploadAsync(stream, "image/png", "u1", CancellationToken.None);

            result.Should().Be("https://acc.blob.core.windows.net/avatars/u1.png");
        }

        [Fact]
        public async Task UploadAsync_ShouldSetContentTypeAndCacheControl_Headers()
        {
            var blob = SetupBlobClient("u1.webp");
            using var stream = new MemoryStream();

            await CreateSut().UploadAsync(stream, "image/webp", "u1", CancellationToken.None);

            await blob.Received(1).UploadAsync(
                stream,
                Arg.Is<BlobUploadOptions>(o =>
                    o.HttpHeaders.ContentType == "image/webp" &&
                    o.HttpHeaders.CacheControl == "public, max-age=31536000"),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task UploadAsync_ShouldPropagate_CancellationToken()
        {
            var blob = SetupBlobClient("u1.jpg");
            using var stream = new MemoryStream();
            using var cts = new CancellationTokenSource();

            await CreateSut().UploadAsync(stream, "image/jpeg", "u1", cts.Token);

            await blob.Received(1).UploadAsync(stream, Arg.Any<BlobUploadOptions>(), cts.Token);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDelete_AllBlobsMatchingUserIdPrefix()
        {
            _container
                .GetBlobsAsync(BlobTraits.None, BlobStates.None, "u1.", Arg.Any<CancellationToken>())
                .Returns(BuildPageable("u1.png", "u1.jpg"));
            _container
                .DeleteBlobIfExistsAsync(Arg.Any<string>(), cancellationToken: Arg.Any<CancellationToken>())
                .Returns(Task.FromResult(Substitute.For<Response<bool>>()));

            await CreateSut().DeleteAsync("u1", CancellationToken.None);

            await _container.Received(1).DeleteBlobIfExistsAsync("u1.png", cancellationToken: Arg.Any<CancellationToken>());
            await _container.Received(1).DeleteBlobIfExistsAsync("u1.jpg", cancellationToken: Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task DeleteAsync_WithNoMatchingBlobs_ShouldNotCallDelete()
        {
            _container
                .GetBlobsAsync(BlobTraits.None, BlobStates.None, "ghost.", Arg.Any<CancellationToken>())
                .Returns(BuildPageable());

            await CreateSut().DeleteAsync("ghost", CancellationToken.None);

            await _container.DidNotReceive().DeleteBlobIfExistsAsync(Arg.Any<string>(), cancellationToken: Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task DeleteAsync_ShouldPropagate_CancellationToken()
        {
            using var cts = new CancellationTokenSource();
            _container
                .GetBlobsAsync(BlobTraits.None, BlobStates.None, "u1.", cts.Token)
                .Returns(BuildPageable("u1.png"));
            _container
                .DeleteBlobIfExistsAsync(Arg.Any<string>(), cancellationToken: cts.Token)
                .Returns(Task.FromResult(Substitute.For<Response<bool>>()));

            await CreateSut().DeleteAsync("u1", cts.Token);

            _container.Received(1).GetBlobsAsync(BlobTraits.None, BlobStates.None, "u1.", cts.Token);
            await _container.Received(1).DeleteBlobIfExistsAsync("u1.png", cancellationToken: cts.Token);
        }

        private static AsyncPageable<BlobItem> BuildPageable(params string[] names)
        {
            var items = names.Select(n => BlobsModelFactory.BlobItem(name: n)).ToList();
            var page = Page<BlobItem>.FromValues(items, continuationToken: null, response: Substitute.For<Response>());
            return AsyncPageable<BlobItem>.FromPages(new[] { page });
        }
    }
}
