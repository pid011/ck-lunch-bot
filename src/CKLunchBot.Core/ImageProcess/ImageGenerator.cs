// Copyright (c) Sepi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;

namespace CKLunchBot.Core.ImageProcess
{
    internal class ImageGenerator : IDisposable
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

        public ImageGenerator DrawText(Font font, Color color, Vector2 position, string text, HorizontalAlignment align = 0)
        {
            var drawingOptions = new DrawingOptions()
            {
                GraphicsOptions = new GraphicsOptions
                {
                    Antialias = true
                }
            };

            var textOptions = new TextOptions(font)
            {
                HorizontalAlignment = align,
                VerticalAlignment = VerticalAlignment.Top,
                Origin = position,
                ApplyHinting = true
            };

            _image.Mutate(x => x.DrawText(drawingOptions, textOptions, text, Brushes.Solid(color), null));

            return this;
        }

        public ImageGenerator AddFont(string name, string path, float size, FontStyle style)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new Exception("Font name is cannot be empty or null.");
            }

            var collection = new FontCollection();
            FontFamily family = collection.Add(path);
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

        #region IDisposable

        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _image.Dispose();
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ImageGenerator()
        {
            Dispose(false);
        }

        #endregion IDisposable
    }
}
