// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace CKLunchBot.Core.ImageProcess
{
    public class ImageGenerator : IDisposable
    {
        public Dictionary<string, Font> Fonts { get; } = new Dictionary<string, Font>();

        private readonly Image _image;

        public ImageGenerator(string imagePath)
        {
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException($"Image file does not exist. path: {imagePath}");
            }

            _image = Image.Load(imagePath);
        }

        public ImageGenerator DrawText(
            (float x, float y) position, Font font, (byte r, byte g, byte b) color, string text, HorizontalAlignment align = 0)
        {
            TextGraphicsOptions options = new TextGraphicsOptions()
            {
                GraphicsOptions = new GraphicsOptions
                {
                    Antialias = true
                },
                TextOptions = new TextOptions
                {
                    ApplyKerning = true,
                    HorizontalAlignment = align,
                    VerticalAlignment = VerticalAlignment.Top
                }
            };

            _image.Mutate(x =>
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
            _image.SaveAsPng(stream);
            return stream.ToArray();
        }

        #region IDisposable Support

        private bool _disposedValue = false; // 중복 호출을 검색하려면

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                }
                if (_image != null)
                {
                    _image.Dispose();
                }
                _disposedValue = true;
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
