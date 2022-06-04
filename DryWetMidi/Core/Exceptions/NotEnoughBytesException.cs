using System;
using System.Runtime.Serialization;
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
    [Serializable]
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

        private NotEnoughBytesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ExpectedCount = info.GetInt64(nameof(ExpectedCount));
            ActualCount = info.GetInt64(nameof(ActualCount));
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

        #region Overrides

        /// <summary>
        /// Sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <c>null</c>.</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(ExpectedCount), ExpectedCount);
            info.AddValue(nameof(ActualCount), ActualCount);
        }

        #endregion
    }
}
