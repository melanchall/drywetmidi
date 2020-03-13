using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Settings according to which <see cref="MidiReader"/> should read MIDI data.
    /// </summary>
    public sealed class ReaderSettings
    {
        #region Fields

        private int _nonSeekableStreamBufferSize = 1024;
        private int _nonSeekableStreamIncrementalBytesReadingThreshold = 16384;
        private int _nonSeekableStreamIncrementalBytesReadingStep = 2048;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets internal buffer for reading MIDI data from non-seekable stream.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Value is zero or negative.</exception>
        /// <remarks>
        /// During reading MIDI data there are cases when reader should move current stream's position back.
        /// If stream doesn't support seeking, it will fail. So for non-seekable stream reading engine
        /// should maintain buffer of last N bytes to have ability to jump back. That N value is controlled by
        /// this property.
        /// </remarks>
        public int NonSeekableStreamBufferSize
        {
            get { return _nonSeekableStreamBufferSize; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Value is zero or negative.");

                _nonSeekableStreamBufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets minimum count of bytes to read them from non-seekable stream incrementally instead of
        /// reading them all at once.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Value is zero or negative.</exception>
        /// <remarks>
        /// If count of bytes to read is greater than or equal to the value of this property, reading engine
        /// will read data by chunks of N bytes where N is a value of <see cref="NonSeekableStreamIncrementalBytesReadingStep"/>
        /// property. This property applies for non-seekable streams only. Since there is no way to determine available count
        /// of bytes within such streams, incremental reading can prevent <see cref="OutOfMemoryException"/>.
        /// </remarks>
        public int NonSeekableStreamIncrementalBytesReadingThreshold
        {
            get { return _nonSeekableStreamIncrementalBytesReadingThreshold; }
            set
            {
                ThrowIfArgument.IsNegative(nameof(value), value, "Value is negative.");

                _nonSeekableStreamIncrementalBytesReadingThreshold = value;
            }
        }

        /// <summary>
        /// Gets or sets size of chunk for incremental reading of MIDI data from non-seekable stream.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Value is zero or negative.</exception>
        /// <remarks>
        /// This property works in conjunction with <see cref="NonSeekableStreamIncrementalBytesReadingThreshold"/>.
        /// </remarks>
        public int NonSeekableStreamIncrementalBytesReadingStep
        {
            get { return _nonSeekableStreamIncrementalBytesReadingStep; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Value is zero or negative.");

                _nonSeekableStreamIncrementalBytesReadingStep = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether all MIDI data should be put to memory and read from there.
        /// The default value is false.
        /// </summary>
        /// <remarks>
        /// If the property set to true, it can significantly speed up reading MIDI data. For almost all real
        /// MIDI files it shouldn't be a problem to place entire file to memory since the size of most MIDI
        /// files is relatively small.
        /// </remarks>
        public bool ReadFromMemory { get; set; }

        #endregion
    }
}
