namespace Melanchall.DryWetMidi.Common
{
    internal static class DataTypesParsers
    {
        public static readonly ShortByteParser FourBitNumberParser = new ShortByteParser(0, 15);

        public static readonly ShortByteParser SevenBitNumberParser = new ShortByteParser(0, 127);
    }
}
