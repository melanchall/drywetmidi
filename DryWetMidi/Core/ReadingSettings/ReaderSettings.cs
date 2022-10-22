using System;
using System.ComponentModel;
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
        
        private int _bufferSize = 4096;
        private BufferingPolicy _bufferingPolicy = BufferingPolicy.UseFixedSizeBuffer;

        private int _bytesPacketMaxLength = 4096;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets internal buffer for reading MIDI data from non-seekable stream.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Value is zero or negative.</exception>
        /// <remarks>
        /// <para>During reading MIDI data there are cases when reader should move current stream's position back.
        /// If stream doesn't support seeking, it will fail. So for non-seekable stream reading engine
        /// should maintain buffer of last N bytes to have ability to jump back. That N value is controlled by
        /// this property.</para>
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
        /// <para>If count of bytes to read is greater than or equal to the value of this property, reading engine
        /// will read data by chunks of N bytes where N is a value of <see cref="NonSeekableStreamIncrementalBytesReadingStep"/>
        /// property. This property applies for non-seekable streams only. Since there is no way to determine available count
        /// of bytes within such streams, incremental reading can prevent <see cref="OutOfMemoryException"/>.</para>
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
        /// <para>This property works in conjunction with <see cref="NonSeekableStreamIncrementalBytesReadingThreshold"/>.</para>
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
        /// Gets or sets a rule according to which MIDI data should be buffered before reading.
        /// The default is <see cref="BufferingPolicy.UseFixedSizeBuffer"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public BufferingPolicy BufferingPolicy
        {
            get { return _bufferingPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _bufferingPolicy = value;
            }
        }

        /// <summary>
        /// Gets or sets the size of a buffer that will be used by <see cref="MidiReader"/> in case of
        /// <see cref="BufferingPolicy.UseFixedSizeBuffer"/> policy used for <see cref="BufferingPolicy"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero or negative.</exception>
        public int BufferSize
        {
            get { return _bufferSize; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Value is zero or negative.");

                _bufferSize = value;
            }
        }

        /// <summary>
        /// Gets or sets the buffer that will be used by <see cref="MidiReader"/> in case of
        /// <see cref="BufferingPolicy.UseCustomBuffer"/> policy used for <see cref="BufferingPolicy"/>.
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// Gets or sets the maximum length of a bytes packet that will be used by <see cref="MidiTokensReader"/>.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is zero or negative.</exception>
        public int BytesPacketMaxLength
        {
            get { return _bytesPacketMaxLength; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "Value is zero or negative.");

                _bytesPacketMaxLength = value;
            }
        }

        #endregion
    }
}
