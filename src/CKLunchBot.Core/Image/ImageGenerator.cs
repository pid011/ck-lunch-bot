using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace CKLunchBot.Core.Image
{
    public class ImageGenerator : IDisposable
    {
        public readonly string FontsPath = Path.Combine("assets", "fonts");
        public Dictionary<string, Font> Fonts { get; } = new Dictionary<string, Font>();
        public Bitmap ImageSource { get; }

        private Graphics graphics;

        public ImageGenerator(string imagePath)
        {
            //  TODO: ImageSource의 타입을 Image로 바꿔도 원본파일이 수정되지 않는지 확인
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"Image file does not exist. path: {imagePath}");
            }
            ImageSource = new Bitmap(imagePath);
            graphics = Graphics.FromImage(ImageSource);
        }

        public void DrawText((float x, float y) position, Font font, (int r, int g, int b) rgbColor, string text,
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

        public byte[] ToByteArray(ImageFormat format)
        {
            using var stream = new MemoryStream();
            ImageSource.Save(stream, format);
            return stream.ToArray();
        }

        public void Dispose()
        {
            foreach (var font in Fonts)
            {
                font.Value.Dispose();
            }
            ImageSource.Dispose();
            graphics.Dispose();
        }
    }
}