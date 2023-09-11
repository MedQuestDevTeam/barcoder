namespace Barcoder
{
    public interface IBarcodeIntCs : IBarcode
    {
        int Checksum { get; }
    }
}
