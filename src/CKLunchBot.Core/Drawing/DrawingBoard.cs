using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace CKLunchBot.Core.Drawing;

internal class DrawingBoard : IDisposable
{
    public Dictionary<string, Font> Fonts { get; } = new Dictionary<string, Font>();

    private readonly Image _image;
    private readonly Queue<Action<IImageProcessingContext>> _actions = new();

    private readonly object _fontsLock = new();
    private readonly object _actionsLock = new();

    public DrawingBoard(string templateImagePath)
    {
        if (!File.Exists(templateImagePath))
        {
            throw new FileNotFoundException($"Template image file does not exist. path: {templateImagePath}");
        }

        _image = Image.Load(templateImagePath);
    }

    public DrawingBoard AddFont(string name, string path, float size, FontStyle style)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new Exception("Font name is cannot be empty or null.");
        }

        var collection = new FontCollection();
        FontFamily family = collection.Add(path);
        Font font = family.CreateFont(size, style);

        lock (_fontsLock)
        {
            Fonts.Add(name, font);
        }

        return this;
    }

    public DrawingBoard DrawText(Font font, SolidBrush brush, PointF location, string text, HorizontalAlignment alignment = HorizontalAlignment.Left)
    {
        var drawingOptions = new DrawingOptions()
        {
            GraphicsOptions = new GraphicsOptions
            {
                Antialias = true,
            }
        };

        var textOptions = new TextOptions(font)
        {
            Origin = location,
            VerticalAlignment = VerticalAlignment.Top,
            HorizontalAlignment = alignment,
        };

        lock (_actionsLock)
        {
            _actions.Enqueue(x => x.DrawText(drawingOptions, textOptions, text, brush, null));
        }
        return this;
    }

    public DrawingBoard Mutate()
    {
        lock (_actionsLock)
        {
            _image.Mutate(x =>
            {
                while (_actions.TryDequeue(out var action))
                {
                    action(x);
                }
            });
        }

        return this;
    }

    public async Task<byte[]> ExportAsPngAsync(CancellationToken cancellation = default)
    {
        var stream = new MemoryStream();

        await _image.SaveAsPngAsync(stream, cancellation);
        var bytes = stream.ToArray();

        await stream.DisposeAsync();
        return bytes;
    }

    public async Task<byte[]> ExportAsJpegAsync(int quality = 75, CancellationToken cancellation = default)
    {
        var stream = new MemoryStream();

        var encoder = new JpegEncoder { Quality = quality };
        await _image.SaveAsJpegAsync(stream, encoder, cancellation);
        var bytes = stream.ToArray();

        await stream.DisposeAsync();
        return bytes;
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
            Fonts.Clear();
            _actions.Clear();
            _image.Dispose();
        }

        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~DrawingBoard()
    {
        Dispose(false);
    }

    #endregion IDisposable
}
