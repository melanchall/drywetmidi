using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when a MIDI file doesn't contain enough bytes
    /// to read a value.
    /// </summary>
    /// <remarks>
    /// <para>Note that this exception will be thrown only if <see cref="ReadingSettings.NotEnoughBytesPolicy"/>
    /// is set to <see cref="NotEnoughBytesPolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.</para>
    /// </remarks>
    public sealed class NotEnoughBytesException : MidiException
    {
        #region Constructors

        internal NotEnoughBytesException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        internal NotEnoughBytesException(string message, long expectedCount, long actualCount)
            : base(message)
        {
            ExpectedCount = expectedCount;
            ActualCount = actualCount;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the expected count of bytes.
        /// </summary>
        public long ExpectedCount { get; }

        /// <summary>
        /// Gets the actual count of bytes available in the reader's underlying stream.
        /// </summary>
        public long ActualCount { get; }

        #endregion
    }
}
