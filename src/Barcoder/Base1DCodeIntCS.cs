using Barcoder.Utils;

namespace Barcoder
{
    public class Base1DCodeIntCs : Base1DCode, IBarcodeIntCs
    {
        internal Base1DCodeIntCs(BitList bitList, string kind, string content, int checksum, int margin)
            : base(bitList, kind, content, margin)
        {
            Checksum = checksum;
        }

        public int Checksum { get; }
    }
}
