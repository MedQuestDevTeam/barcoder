using System.Drawing;
using System.Numerics;
using SkiaSharp;

namespace Barcoder.Renderer.Image.Internal
{
    internal static class EanContentRenderer
    {
        private const int UnscaledFontSize = 9;
        private const int ContentMargin = 9;
        private const int ContentVerticalOffset = 0;

        public static void Render(SkiaSharp.SKSurface image, IBarcode barcode, string fontFamily, int scale)
        {
            // SkiaSharp.SKFont font = SystemFonts.CreateFont(fontFamily, UnscaledFontSize * scale, FontStyle.Regular);
            SkiaSharp.SKFont font = new SKFont(SKTypeface.FromFamilyName(fontFamily), UnscaledFontSize * scale);

            switch (barcode.Metadata.CodeKind)
            {
            case BarcodeType.Ean8:
                RenderContentForEan8(image, barcode.Content, font, barcode.Margin, scale);
                break;
            case BarcodeType.Ean13:
                RenderContentForEan13(image, barcode.Content, font, barcode.Margin, scale);
                break;
            }
        }

        private static void RenderContentForEan8(SKSurface image, string content, SKFont font, int margin, int scale)
        {
            var height = image.Canvas.DeviceClipBounds.Height;
            var width = image.Canvas.DeviceClipBounds.Width;
            int ApplyScale(int value) => value * scale;
            RenderWhiteRect(image, ApplyScale(margin + 3), image.Canvas.DeviceClipBounds.Height - ApplyScale(margin + ContentMargin), ApplyScale(29), ApplyScale(ContentMargin));
            RenderWhiteRect(image, ApplyScale(margin + 35), image.Canvas.DeviceClipBounds.Height - ApplyScale(margin + ContentMargin), ApplyScale(29), ApplyScale(ContentMargin));

            float textTop = image.Canvas.DeviceClipBounds.Height - (margin + ContentMargin / 2.0f - ContentVerticalOffset) * scale;
            float textCenter1 = (29.0f / 2.0f + margin + 3.0f) * scale;
            float textCenter2 = (29.0f / 2.0f + margin + 35.0f) * scale;
            RenderBlackText(image, content.Substring(0, 4), textCenter1, textTop, font);
            RenderBlackText(image, content.Substring(4), textCenter2, textTop, font);
        }

        private static void RenderContentForEan13(SKSurface image, string content, SKFont font, int margin, int scale)
        {
            int ApplyScale(int value) => value * scale;
            RenderWhiteRect(image, ApplyScale(margin + 3), image.Canvas.DeviceClipBounds.Height - ApplyScale(margin + ContentMargin), ApplyScale(43), ApplyScale(ContentMargin));
            RenderWhiteRect(image, ApplyScale(margin + 49), image.Canvas.DeviceClipBounds.Height - ApplyScale(margin + ContentMargin), ApplyScale(43), ApplyScale(ContentMargin));

            float textTop = image.Canvas.DeviceClipBounds.Height - (margin + ContentMargin / 2.0f - ContentVerticalOffset) * scale;
            float textCenter1 = (margin - 4.0f) * scale;
            float textCenter2 = (43.0f / 2.0f + margin + 3.0f) * scale;
            float textCenter3 = (43.0f / 2.0f + margin + 49.0f) * scale;
            RenderBlackText(image, content.Substring(0, 1), textCenter1, textTop, font);
            RenderBlackText(image, content.Substring(1, 6), textCenter2, textTop, font);
            RenderBlackText(image, content.Substring(7), textCenter3, textTop, font);
        }

        private static void RenderWhiteRect(SKSurface image, int x, int y, int width, int height)
        {
            image.Canvas.DrawRect( new SKRect(x, y, x + width, y + height), new SKPaint { Color = SKColors.White });

        }

        private static void RenderBlackText(SKSurface image, string text, float x, float y, SKFont font)
        {
            var info = image.Canvas.DeviceClipBounds;
            using (var paint = new SKPaint() { Color = SKColors.Black, IsAntialias = true, Style = SKPaintStyle.Fill })
            {
                image.Canvas.DrawText("SkiaSharp", info.Width / 2, (info.Height + font.Size) / 2, font, paint);    
            }
            
        }
    }
}
