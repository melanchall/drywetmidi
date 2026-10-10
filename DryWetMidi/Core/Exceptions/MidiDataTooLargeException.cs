using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when size of MIDI data to read exceeds
    /// <see cref="ReadingSettings.MaxDataSize"/>.
    /// </summary>
    public sealed class MidiDataTooLargeException : MidiException
    {
        #region Constructors

        internal MidiDataTooLargeException(long dataSize, long maxDataSize)
            : base($"Size of MIDI data ({dataSize} bytes) exceeds the maximum allowed ({maxDataSize} bytes).")
        {
            DataSize = dataSize;
            MaxDataSize = maxDataSize;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the actual size of the data in bytes.
        /// </summary>
        public long DataSize { get; }

        /// <summary>
        /// Gets the maximum allowed size of the data in bytes.
        /// </summary>
        public long MaxDataSize { get; }

        #endregion
    }
}
