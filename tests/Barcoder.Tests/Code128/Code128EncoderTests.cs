using System;
using Barcoder.Code128;
using FluentAssertions;
using Xunit;

namespace Barcoder.Tests.Code128
{
    public sealed class Code128EncoderTests
    {
        private const string Fnc1      = "\u00f1";
        private const string Fnc2      = "\u00f2";
        private const string Fnc3      = "\u00f3";
        private const string Fnc4      = "\u00f4";
        private const string EncFnc1   = "11110101110";
        private const string EncCodeC  = "10111011110";
        private const string EncCodeB  = "10111101110";
        private const string EncFnc2   = "11110101000";
        private const string EncFnc3   = "10111100010";
        private const string EncFnc4   = "10111101110";
        private const string EncStartB = "11010010000";
        private const string EncStop   = "1100011101011";

        // Special Case FC1 can also be encoded to C Table therefor using 123 as suffix might have unexpected results.
        [Theory]
        [InlineData(Fnc1 + "A23", EncStartB + EncFnc1 + "10100011000" + "11001110010" + "11001011100" + "10100011110" + EncStop)]
        [InlineData(Fnc2 + "123", EncStartB + EncFnc2 + "10011100110" + "11001110010" + "11001011100" + "11100010110" + EncStop)]
        [InlineData(Fnc3 + "123", EncStartB + EncFnc3 + "10011100110" + "11001110010" + "11001011100" + "11101000110" + EncStop)]
        [InlineData(Fnc4 + "123", EncStartB + EncFnc4 + "10011100110" + "11001110010" + "11001011100" + "11100011010" + EncStop)]
        [InlineData(Fnc3 + "$P\rI", "110100001001011110001010010001100111011101101111011101011000100010110001010001100011101011")]
        // <Start A><FNC3>$P\r<checksum><STOP>
        [InlineData(Fnc3 + "$P\r",
            "11010000100" + // <Start A>
            "10111100010" + // <FNC3>
            "10010001100" + // $
            "11101110110" + // P
            "11110111010" + // CR
            "11000100010" + // checksum = 'I'
            "1100011101011")] // STOP
        // <Start B><FNC3>$P,Ae,P<CR><checksum><STOP>
        [InlineData(Fnc3 + "$P,Ae,P\r",
            "11010010000" + // <Start B>
            "10111100010" + // <FNC3>
            "10010001100" + // $
            "11101110110" + // P
            "10110011100" + // ,
            "10100011000" + // A
            "10110010000" + // e
            "10110011100" + // ,
            "11101110110" + // P
            "11101011110" + // <Code A>
            "11110111010" + // <CR>
            "10110001000" + // checksum = 'D'
            "1100011101011")] // STOP
        public void Encode(string txt, string testResult)
        {
            IBarcodeIntCs code = Code128Encoder.Encode(txt);

            code.Bounds.X.Should().Be(testResult.Length);
            code.Bounds.Y.Should().Be(1);
            code.Metadata.CodeKind.Should().Be(BarcodeType.Code128);
            code.Metadata.Dimensions.Should().Be(1);

            string encoded = string.Empty;
            int i = 0;
            foreach (var r in testResult)
                encoded += code.At(i++, 0) ? "1" : "0";
            encoded.Should().Be(testResult);
        }

        [Theory]
        [InlineData("(241)ABC(10)123", 
            EncStartB + 
            EncFnc1 + 
            "11001110010" + // 2
            "11001001110" + // 4
            "10011100110" + // 1
            "10100011000" + // A
            "10001011000" + // B
            "10001000110" + // C
            EncCodeC + 
            EncFnc1 + 
            "11001000100" + // 10
            "10110011100" + // 12
            EncCodeB + 
            "11001011100" + // 3
            "10001110110" + // Checksum = 'O'
            EncStop)]
        public void Encode_GS1(string txt, string testResult)
        {
            IBarcodeIntCs code = Code128Encoder.Encode(txt, gs1ModeEnabled: true);

            code.Bounds.X.Should().Be(testResult.Length);
            code.Bounds.Y.Should().Be(1);
            code.Metadata.CodeKind.Should().Be(BarcodeType.Code128);
            code.Metadata.Dimensions.Should().Be(1);

            string encoded = string.Empty;
            int i = 0;
            foreach (var r in testResult)
                encoded += code.At(i++, 0) ? "1" : "0";
            encoded.Should().Be(testResult);
        } 

        [Fact]
        public void Encode_ShouldUseCTable()
        {
            bool result;
            bool T(byte currentEncoding, params char[] nextChars)
                => Code128Encoder.ShouldUseCTable(nextChars, currentEncoding);

            result = T(Constants.StartCSymbol, Constants.Fnc1, '1', '2');
            result.Should().BeTrue();

            result = T(Constants.StartCSymbol, Constants.Fnc1, '1');
            result.Should().BeFalse();

            result = T(Constants.StartCSymbol, '0', Constants.Fnc1, '1');
            result.Should().BeFalse();

            result = T(Constants.StartBSymbol, '0', '1', Constants.Fnc1, '2', '3');
            result.Should().BeTrue();

            result = T(Constants.StartBSymbol, '0', '1', Constants.Fnc1);
            result.Should().BeFalse();
        }

        [Fact]
        public void Encode_ShouldUseATable()
        {
            bool result;
            bool T(byte currentEncoding, params char[] nextChars)
                => Code128Encoder.ShouldUseATable(nextChars, currentEncoding);

            result = T(0, '\r', 'A');
            result.Should().BeTrue();

            result = T(0, Constants.Fnc1, '\r');
            result.Should().BeTrue();

            result = T(0, Constants.Fnc1, '1', '2', '3');
            result.Should().BeFalse();
        }

        [Fact]
        public void Encode_EmptyString_ShouldThrowException()
        {
            Action action = () => Code128Encoder.Encode(string.Empty);
            action.Should().Throw<ArgumentException>()
                .And.Message.StartsWith("Content length should be between 1 and 80 but got 0");
        }

        [Fact]
        public void Encode_ContentTooLong_ShouldThrowException()
        {
            Action action = () => Code128Encoder.Encode("123456789012345678901234567890123456789012345678901234567890123456789012345678901");
            action.Should().Throw<ArgumentException>()
                .And.Message.StartsWith("Content length should be between 1 and 80 but got 81");
        }
    }
}
