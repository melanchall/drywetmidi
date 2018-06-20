using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// The exception that is thrown when a MIDI file doesn't contain enough bytes
    /// to read a value.
    /// </summary>
    /// <remarks>
    /// Note that this exception will be thrown only if <see cref="ReadingSettings.NotEnoughBytesPolicy"/>
    /// is set to <see cref="NotEnoughBytesPolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.
    /// </remarks>
    [Serializable]
    public sealed class NotEnoughBytesException : MidiException
    {
        #region Constants

        private const string ExpectedCountSerializationPropertyName = "ExpectedCount";
        private const string ActualCountSerializationPropertyName = "ActualCount";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NotEnoughBytesException"/>.
        /// </summary>
        public NotEnoughBytesException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotEnoughBytesException"/> with the
        /// specified error message.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        public NotEnoughBytesException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotEnoughBytesException"/> class with the
        /// specified error message and a reference to the inner exception that is the
        /// cause of this exception.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception,
        /// or a null reference if no inner exception is specified.</param>
        public NotEnoughBytesException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotEnoughBytesException"/> with the
        /// specified error message, expected count of bytes and the actual one available in
        /// the reader's underlying stream.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="expectedCount">Expected count of bytes.</param>
        /// <param name="actualCount">Actual count of bytes available in the reader's underlying stream.</param>
        public NotEnoughBytesException(string message, long expectedCount, long actualCount)
            : this(message)
        {
            ExpectedCount = expectedCount;
            ActualCount = actualCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotEnoughBytesException"/>
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        private NotEnoughBytesException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ExpectedCount = info.GetInt64(ExpectedCountSerializationPropertyName);
            ActualCount = info.GetInt64(ActualCountSerializationPropertyName);
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
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ThrowIfArgument.IsNull(nameof(info), info);

            info.AddValue(ExpectedCountSerializationPropertyName, ExpectedCount);
            info.AddValue(ActualCountSerializationPropertyName, ActualCount);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}
