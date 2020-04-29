using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace CKLunchBot.Core.Image
{
    public abstract class ImageGenerator : IDisposable
    {
        public readonly string FontsPath = Path.Combine("assets", "fonts");
        public Dictionary<string, Font> Fonts { get; } = new Dictionary<string, Font>();
        public Bitmap ImageSource { get; }

        private readonly Graphics graphics;

        private bool disposed;

        protected ImageGenerator(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"Image file does not exist. path: {imagePath}");
            }
            ImageSource = new Bitmap(imagePath);
            graphics = Graphics.FromImage(ImageSource);

            // Set Antialiasing mode
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
        }

        /// <summary>
        /// Generate an image and return it as a byte array.
        /// </summary>
        /// <returns>image as a byte array</returns>
        public abstract byte[] Generate();

        protected void DrawText((float x, float y) position, Font font, (int r, int g, int b) rgbColor, string text,
                             StringAlignment alignment = StringAlignment.Near)
        {
            try
            {
                var color = Color.FromArgb(rgbColor.r, rgbColor.g, rgbColor.b);
                var format = new StringFormat() { Alignment = alignment, LineAlignment = StringAlignment.Near };
                graphics.DrawString(text, font, new SolidBrush(color), new PointF(position.x, position.y), format);
            }
            catch (ArgumentException)
            {
                throw;
            }
        }

        protected byte[] ToByteArray(ImageFormat format)
        {
            using var stream = new MemoryStream();
            ImageSource.Save(stream, format);
            return stream.ToArray();
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            foreach (var font in Fonts)
            {
                if (font.Value != null) font.Value.Dispose();
            }
            ImageSource.Dispose();
            if (graphics != null) graphics.Dispose();

            disposed = true;
        }
    }
}