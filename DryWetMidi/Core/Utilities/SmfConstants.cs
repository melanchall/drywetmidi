using System.Text;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Constants related to Standard MIDI Files.
    /// </summary>
    public static class SmfConstants
    {
        #region Properties

        /// <summary>
        /// Gets the default <see cref="Encoding"/> used by Standard MIDI File to encode/decode
        /// text data.
        /// </summary>
        public static Encoding DefaultTextEncoding => Encoding.ASCII;

        #endregion
    }
}
