using System.Text;

namespace Melanchall.DryWetMidi.Smf
{
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
