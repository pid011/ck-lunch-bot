using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

using System;
using System.Collections.Generic;
using System.IO;

namespace CKLunchBot.Core.ImageProcess
{
    public class ImageGenerator : IDisposable
    {
        public Dictionary<string, Font> Fonts { get; } = new Dictionary<string, Font>();

        private readonly Image image;

        public ImageGenerator(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"Image file does not exist. path: {imagePath}");
            }

            image = Image.Load(imagePath);
        }

        public ImageGenerator DrawText(
            (float x, float y) position, Font font, (byte r, byte g, byte b) color, string text, HorizontalAlignment align = 0)
        {
            if (image.IsDisposed)
            {
                throw new ObjectDisposedException(nameof(image));
            }

            TextGraphicsOptions options = new TextGraphicsOptions()
            {
                Antialias = true,
                ApplyKerning = true,
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Top,
                //WrapTextWidth = 100
            };

            image.Mutate(x =>
                x.DrawText(options, text, font, Color.FromRgb(color.r, color.g, color.b), new PointF(position.x, position.y)));

            return this;
        }

        public ImageGenerator AddFont(string name, string path, float size, FontStyle style)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Thefont name is cannot be empty or null.");
            }

            var collection = new FontCollection();
            FontFamily family = collection.Install(path);
            Font font = family.CreateFont(size, style);
            Fonts.Add(name, font);

            return this;
        }

        public byte[] ExportAsPng()
        {
            using var stream = new MemoryStream();
            image.SaveAsPng(stream);
            return stream.ToArray();
        }

        #region IDisposable Support

        private bool disposedValue = false; // 중복 호출을 검색하려면

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                if (image != null)
                {
                    image.Dispose();
                }
                disposedValue = true;
            }
        }

        ~ImageGenerator()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support
    }
}