using System;
using System.IO;
using System.Numerics;
using Barcoder.Renderer.Image.Internal;
using Barcoder.Renderers;
using SkiaSharp;


namespace Barcoder.Renderer.Image
{
    public sealed class ImageRenderer : IRenderer
    {
        private readonly ImageRendererOptions _options;

        public ImageRenderer(ImageRendererOptions options = null)
        {
            options = options ?? new ImageRendererOptions();

            if (options.PixelSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(options.PixelSize), "Value must be larger than zero");
            if (options.BarHeightFor1DBarcode <= 0)
                throw new ArgumentOutOfRangeException(nameof(options.BarHeightFor1DBarcode),
                    "Value must be larger than zero");
            if (options.JpegQuality < 0 || options.JpegQuality > 100)
                throw new ArgumentOutOfRangeException(nameof(options.JpegQuality),
                    "Value must be a value between 0 and 100");
            
            if (options.ImageFormat == SKEncodedImageFormat.Bmp || options.ImageFormat == SKEncodedImageFormat.Gif)
                throw new ArgumentOutOfRangeException(nameof(options.ImageFormat),
                    "format cannot be BMP or GIF");

            _options = options;
        }


        public void Render(IBarcode barcode, Stream outputStream)
        {
            barcode = barcode ?? throw new ArgumentNullException(nameof(barcode));
            outputStream = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
            if (barcode.Bounds.Y == 1)
                Render1D(barcode, outputStream);
            else if (barcode.Bounds.Y > 1)
                Render2D(barcode, outputStream);
            else
                throw new NotSupportedException($"Y value of {barcode.Bounds.Y} is invalid");
        }

        private void Render1D(IBarcode barcode, Stream outputStream)
        {
            int margin = _options.CustomMargin ?? barcode.Margin;
            int width = (barcode.Bounds.X + 2 * margin) * _options.PixelSize;
            int height = (_options.BarHeightFor1DBarcode + 2 * margin) * _options.PixelSize;

            var info = new SKImageInfo(width, height);
            using( var image = SKSurface.Create(info))
            {
                image.Canvas.Clear(SKColors.White);
                for (var x = 0; x < barcode.Bounds.X; x++)
                {
                    if (!barcode.At(x, 0))
                        continue;
                    image.Canvas.DrawRect(
                        new SKRect((margin + x) * _options.PixelSize, margin * _options.PixelSize,
                            (margin + x + 1) * _options.PixelSize,
                            (_options.BarHeightFor1DBarcode + margin) * _options.PixelSize),
                        new SKPaint { Color = SKColors.Black });
                }

                if (_options.IncludeEanContentAsText && barcode.IsEanBarcode())
                    EanContentRenderer.Render(image, barcode, fontFamily: _options.EanFontFamily,
                        scale: _options.PixelSize);


                image.Snapshot().Encode(_options.ImageFormat, 100).SaveTo(outputStream);
            }
        }

        private void Render2D(IBarcode barcode, Stream outputStream)
        {
            int margin = _options.CustomMargin ?? barcode.Margin;
            int width = (barcode.Bounds.X + 2 * margin) * _options.PixelSize;
            int height = (barcode.Bounds.Y + 2 * margin) * _options.PixelSize;

            var info = new SKImageInfo(width, height);
            using(var image = SKSurface.Create(info)) 
            {
                image.Canvas.Clear(SKColors.White);
                for (var y = 0; y < barcode.Bounds.Y; y++)
                {
                    for (var x = 0; x < barcode.Bounds.X; x++)
                    {
                        if (!barcode.At(x, y)) continue;
                        image.Canvas.DrawRect(
                            new SKRect((margin + x) * _options.PixelSize, (margin + y) * _options.PixelSize,
                                (margin + x + 1) * _options.PixelSize, (margin + y + 1) * _options.PixelSize),
                            new SKPaint { Color = SKColors.Black });
                    }
                }

                image.Snapshot().Encode(_options.ImageFormat, 100).SaveTo(outputStream);
            }
        }
    }
}
