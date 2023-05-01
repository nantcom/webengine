using SkiaSharp;

namespace NC.WebEngine.Core.Favicon
{
    public class FaviconModule : IModule, IService
    {
        private const string FAVICON_LOCATION = "wwwroot\\images\\favicon.png";
        private byte[] imageContent;

        public bool IsFaviconAvailable { get; set; }

        public void Register(WebApplication app)
        {
            // No favicon, no handler
            if (File.Exists(FAVICON_LOCATION) == false)
            {
                return;
            }

            this.IsFaviconAvailable = true;

            imageContent = File.ReadAllBytes(FAVICON_LOCATION);

            app.MapGet("/favicon-{w:int}x{h:int}.{kind}", HandleFavicon);

            app.MapGet("/favicon.ico", (HttpContext httpContext) => HandleFavicon(16, 16, "ico", httpContext));
        }

        public void RegisterBuilder(WebApplicationBuilder builder)
        {
            builder.Services.AddSingleton(this);
        }

        private IResult HandleFavicon(int w, int h, string kind, HttpContext httpContext)
        {
            return new FaviconResult(imageContent,
                kind == "ico" ? "image/ico" : "image/png", w, h);
        }


        public class FaviconResult : IResult
        {
            private byte[] _source;
            private int _newWidth;
            private int _newHeight;
            private string _contentType;

            public FaviconResult(byte[] source, string contentType, int width, int height)
            {
                _source = source;
                _newWidth = width;
                _newHeight = height;
                _contentType = contentType;
            }


            private static Dictionary<string, SKEncodedImageFormat> _imageFormatMapping = new()
            {
                { "image/png", SKEncodedImageFormat.Png },
                { "image/jpeg", SKEncodedImageFormat.Jpeg },
                { "image/jpg", SKEncodedImageFormat.Jpeg },
                { "image/ico", SKEncodedImageFormat.Ico },
                { "image/gif", SKEncodedImageFormat.Gif },
                { "image/webp", SKEncodedImageFormat.Webp },
            };

            private static SKEncodedImageFormat GetImageFormat(string contentType)
            {
                if (_imageFormatMapping.TryGetValue(contentType, out SKEncodedImageFormat result) == true)
                {
                    return result;
                }

                return SKEncodedImageFormat.Webp;
            }


            private void Resize(Stream output)
            {
                using SKBitmap sourceBitmap = SKBitmap.Decode(new ReadOnlySpan<byte>(_source));
                using SKBitmap newBitmap = new SKBitmap(_newWidth, _newHeight);
                sourceBitmap.ScalePixels(newBitmap, SKFilterQuality.High);

                using SKImage scaledImage = SKImage.FromBitmap(newBitmap);

                // Code from : https://gist.github.com/darkfall/1656050

                var format = GetImageFormat(_contentType);
                if (format == SKEncodedImageFormat.Ico)
                {
                    using SKData data = scaledImage.Encode(SKEncodedImageFormat.Png, 100);
                    using MemoryStream ms = new MemoryStream();
                    using MemoryStream outputStream = new MemoryStream();

                    data.SaveTo(ms);

                    using BinaryWriter iconWriter = new BinaryWriter(outputStream);

                    // 0-1 reserved, 0
                    iconWriter.Write((byte)0);
                    iconWriter.Write((byte)0);

                    // 2-3 image type, 1 = icon, 2 = cursor
                    iconWriter.Write((short)1);

                    // 4-5 number of images
                    iconWriter.Write((short)1);

                    // image entry 1
                    // 0 image width
                    iconWriter.Write((byte)_newWidth);
                    // 1 image height
                    iconWriter.Write((byte)_newHeight);

                    // 2 number of colors
                    iconWriter.Write((byte)0);

                    // 3 reserved
                    iconWriter.Write((byte)0);

                    // 4-5 color planes
                    iconWriter.Write((short)0);

                    // 6-7 bits per pixel
                    iconWriter.Write((short)32);

                    // 8-11 size of image data
                    iconWriter.Write((int)ms.Length);

                    // 12-15 offset of image data
                    iconWriter.Write((int)(6 + 16));

                    // write image data
                    // png data must contain the whole png data file
                    iconWriter.Write(ms.ToArray());

                    iconWriter.Flush();

                    outputStream.CopyToAsync(output).Wait();
                }
                else
                {
                    using SKData data = scaledImage.Encode(GetImageFormat(_contentType), 100);
                    data.AsStream(true).CopyToAsync(output).Wait();
                }
            }


            public async Task ExecuteAsync(HttpContext httpContext)
            {
                httpContext.Response.ContentType = _contentType;

                try
                {
                    await Task.Run(() => {
                        this.Resize(httpContext.Response.Body);
                    });
                }
                catch (Exception)
                {
                    httpContext.Response.StatusCode = 500;
                }

                await httpContext.Response.Body.FlushAsync();
            }
        }
    }
}
