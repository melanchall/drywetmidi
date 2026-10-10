using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when size of MIDI data to read exceeds
    /// <see cref="ReadingSettings.MaxMemorySize"/>.
    /// </summary>
    public sealed class MidiFileTooLargeException : MidiException
    {
        #region Constructors

        internal MidiFileTooLargeException(long estimatedSize, long maxSize)
            : base($"Estimated size of the MIDI file in memory ({estimatedSize} bytes) exceeds the maximum allowed ({maxSize} bytes).")
        {
            EstimatedSize = estimatedSize;
            MaxSize = maxSize;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the estimated size of the file in memory in bytes at the moment of the error.
        /// </summary>
        public long EstimatedSize { get; }

        /// <summary>
        /// Gets the maximum allowed size in bytes.
        /// </summary>
        public long MaxSize { get; }

        #endregion
    }
}
