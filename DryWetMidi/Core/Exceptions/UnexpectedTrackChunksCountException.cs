using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when count of track chunks in a MIDI file differs from
    /// the one declared in the header chunk of this file.
    /// </summary>
    /// <remarks>
    /// <para>Note that this exception will be thrown only if <see cref="ReadingSettings.UnexpectedTrackChunksCountPolicy"/>
    /// is set to <see cref="UnexpectedTrackChunksCountPolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.</para>
    /// </remarks>
    public sealed class UnexpectedTrackChunksCountException : MidiException
    {
        #region Constructors

        internal UnexpectedTrackChunksCountException(int expectedCount, int actualCount)
            : base($"Count of track chunks is {actualCount} while {expectedCount} expected.")
        {
            ExpectedCount = expectedCount;
            ActualCount = actualCount;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the expected count of track chunks read from the header chunk.
        /// </summary>
        public int ExpectedCount { get; }

        /// <summary>
        /// Gets the actual count of track chunks read from a MIDI file.
        /// </summary>
        public int ActualCount { get; }

        #endregion
    }
}
