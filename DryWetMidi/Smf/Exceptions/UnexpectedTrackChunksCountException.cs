using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// The exception that is thrown when count of track chunks in a MIDI file differs from
    /// the one declared in the header chunk of this file.
    /// </summary>
    /// <remarks>
    /// Note that this exception will be thrown only if <see cref="ReadingSettings.UnexpectedTrackChunksCountPolicy"/>
    /// is set to <see cref="UnexpectedTrackChunksCountPolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.
    /// </remarks>
    [Serializable]
    public sealed class UnexpectedTrackChunksCountException : MidiException
    {
        #region Constants

        private const string ExpectedCountSerializationPropertyName = "ExpectedCount";
        private const string ActualCountSerializationPropertyName = "ActualCount";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedTrackChunksCountException"/>.
        /// </summary>
        public UnexpectedTrackChunksCountException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedTrackChunksCountException"/> with the
        /// specified error message, expected count of track chunks read from the header chunk, and the
        /// actual one.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="expectedCount">Expected count of track chunks read from the header chunk.</param>
        /// <param name="actualCount">Actual count of track chunks read from a MIDI file.</param>
        public UnexpectedTrackChunksCountException(string message, int expectedCount, int actualCount)
            : base(message)
        {
            ExpectedCount = expectedCount;
            ActualCount = actualCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnexpectedTrackChunksCountException"/>
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        private UnexpectedTrackChunksCountException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ExpectedCount = info.GetInt32(ExpectedCountSerializationPropertyName);
            ActualCount = info.GetInt32(ActualCountSerializationPropertyName);
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
