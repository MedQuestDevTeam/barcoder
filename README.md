# Barcoder - Barcode Encoding Library

[![Build status](https://github.com/huysentruitw/barcoder/actions/workflows/build-test-publish.yml/badge.svg?branch=master)](https://github.com/huysentruitw/barcoder/actions/workflows/build-test-publish.yml?query=branch%3Amaster)

Lightweight Barcode Encoding Library for .NET Framework, .NET Standard and .NET Core. Additional packages are available for rendering the generated barcode to SVG or an image.

Code ported from the GO project https://github.com/boombuler/barcode by [Florian Sundermann](https://github.com/boombuler).

Supported Barcode Types:

* 2 of 5
* Aztec Code
* Codabar
* Code 39
* Code 93
* Code 128
* Code 128 GS1
* Data Matrix (ECC 200)
* Data Matrix GS1
* EAN 8
* EAN 13
* KIX (used by PostNL)  
* PDF 417
* QR Code
* RM4SC (Royal Mail 4 State Code)
* UPC A
* UPC E

## NuGet package

To install the [main package](https://www.nuget.org/packages/Barcoder):

    PM> Install-Package Barcoder

To install the image renderer[^1]:

	PM> Install-Package Barcoder.Renderer.Image
	
## Usage - render to SVG

```csharp
var barcode = Code128Encoder.Encode("FOO/BAR/12345");
var renderer = new SvgRenderer();

using (var stream = new MemoryStream())
using (var reader = new StreamReader(stream))
{
    renderer.Render(barcode, stream);
    stream.Position = 0;

    string svg = reader.ReadToEnd();
    Console.WriteLine(svg);
}
```

## Usage - render to PNG, JPEG

Example for rendering to PNG:

```csharp
var barcode = QrEncoder.Encode("Hello World!");
var renderer = new ImageRenderer(new ImageRendererOptions { ImageFormat = ImageFormat.Png });

using (var stream = new FileStream("output.png", FileMode.Create))
{
    renderer.Render(barcode, stream);
}
```

For supported image formats please reference to SKEncodedImageFormat except **bmp** and **gif**

# Hurrah! Fully MIT licensed!

[^1]: The image renderer is now based on [SkiaSharp](https://github.com/mono/SkiaSharp)
