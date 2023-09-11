namespace Barcoder.Renderer.Image.Internal
{
    internal static class BarcodeExtensions
    {
        public static bool IsEanBarcode(this IBarcode barcode)
            => barcode?.Metadata.CodeKind == BarcodeType.Ean8 || barcode?.Metadata.CodeKind == BarcodeType.Ean13;
    }
}
