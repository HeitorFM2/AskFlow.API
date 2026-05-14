using AskFlow.Application.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace AskFlow.Infrastructure.Services
{
    public class ImageSharpAvatarProcessor : IImageProcessor
    {
        private const int MaxDimension = 512;
        private const string OutputContentType = "image/webp";

        public async Task<ProcessedImage?> ProcessAvatarAsync(Stream content, CancellationToken cancellationToken)
        {
            try
            {
                using var image = await Image.LoadAsync(content, cancellationToken);

                if (image.Width > MaxDimension || image.Height > MaxDimension)
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new Size(MaxDimension, MaxDimension),
                        Mode = ResizeMode.Max
                    }));
                }

                image.Metadata.ExifProfile = null;
                image.Metadata.IccProfile = null;
                image.Metadata.IptcProfile = null;
                image.Metadata.XmpProfile = null;

                var output = new MemoryStream();
                await image.SaveAsync(output, new WebpEncoder(), cancellationToken);
                output.Position = 0;

                return new ProcessedImage(output, OutputContentType);
            }
            catch (UnknownImageFormatException)
            {
                return null;
            }
            catch (InvalidImageContentException)
            {
                return null;
            }
        }
    }
}
