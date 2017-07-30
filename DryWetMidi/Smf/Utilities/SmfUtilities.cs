using System.Text;

namespace Melanchall.DryWetMidi.Smf
{
    internal static class SmfUtilities
    {
        #region Properties

        /// <summary>
        /// Gets the default <see cref="Encoding"/> used by Standard MIDI File which is
        /// <see cref="Encoding.ASCII"/>.
        /// </summary>
        internal static Encoding DefaultEncoding => Encoding.ASCII;

        #endregion
    }
}
