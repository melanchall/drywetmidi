using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Defines how bytes arrays (for example, <see cref="SequencerSpecificEvent.Data"/>) should be
    /// presented in CSV. The default value is <see cref="Decimal"/>.
    /// </summary>
    public enum CsvBytesArrayFormat
    {
        /// <summary>
        /// Bytes should be presented in decimal format. For example, <c>"10 0 123 5"</c>.
        /// </summary>
        Decimal = 0,

        /// <summary>
        /// Bytes should be presented in hexadecimal format. For example, <c>"0A 00 7B 05"</c>.
        /// Note that each value will be in upper case and will have two digits.
        /// </summary>
        Hexadecimal,
    }
}
