using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

using System;
using System.Collections.Generic;
using System.IO;

namespace CKLunchBot.Core.ImageProcess
{
    public abstract class ImageGenerator : IDisposable
    {
        public Dictionary<string, Font> Fonts { get; } = new Dictionary<string, Font>();

        private readonly Image image;

        protected ImageGenerator(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"Image file does not exist. path: {imagePath}");
            }

            image = Image.Load(imagePath);
        }

        /// <summary>
        /// Generate an image and return it as a byte array.
        /// </summary>
        /// <returns>image as a byte array</returns>
        public abstract byte[] Generate(); // TODO: Task<byte[]>로 반환타입 변경하고 사용 시 await ...으로 사용하기

        protected void DrawText(
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
        }

        protected void AddFont(string name, string path, float size, FontStyle style)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Thefont name is cannot be empty or null.");
            }

            var collection = new FontCollection();
            FontFamily family = collection.Install(path);
            Font font = family.CreateFont(size, style);
            Fonts.Add(name, font);
        }

        protected byte[] ExportAsPng()
        {
            using var stream = new MemoryStream();
            image.SaveAsPng(stream);
            return stream.ToArray();
        }

        public void Dispose()
        {
            if (image.IsDisposed)
            {
                return;
            }
            image.Dispose();
        }
    }
}