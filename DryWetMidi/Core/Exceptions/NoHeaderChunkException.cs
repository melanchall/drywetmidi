using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when a MIDI file doesn't contain a header chunk.
    /// </summary>
    /// <remarks>
    /// <para>Note that this exception will be thrown only if <see cref="ReadingSettings.NoHeaderChunkPolicy"/>
    /// is set to <see cref="NoHeaderChunkPolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.</para>
    /// </remarks>
    [Serializable]
    public sealed class NoHeaderChunkException : MidiException
    {
        #region Constructors

        internal NoHeaderChunkException()
            : base("MIDI file doesn't contain the header chunk.")
        {
        }

        #endregion
    }
}
