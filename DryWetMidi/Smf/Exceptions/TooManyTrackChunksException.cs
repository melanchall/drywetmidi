using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// The exception that is thrown while writing a MIDI file when the <see cref="ChunksCollection"/>
    /// contains more than 4294967295 track chunks which is the maximum allowed count for chunks of this type.
    /// </summary>
    [Serializable]
    public sealed class TooManyTrackChunksException : MidiException
    {
        #region Constants

        private const string TrackChunksCountSerializationPropertyName = "TrackChunksCount";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TooManyTrackChunksException"/>.
        /// </summary>
        public TooManyTrackChunksException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TooManyTrackChunksException"/> with the
        /// specified error message and actual track chunks count.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="trackChunksCount">Actual track chunks count.</param>
        public TooManyTrackChunksException(string message, int trackChunksCount)
            : base(message)
        {
            TrackChunksCount = trackChunksCount;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TooManyTrackChunksException"/>
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        private TooManyTrackChunksException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            TrackChunksCount = info.GetInt32(TrackChunksCountSerializationPropertyName);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the actual track chunks count.
        /// </summary>
        public int TrackChunksCount { get; }

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

            info.AddValue(TrackChunksCountSerializationPropertyName, TrackChunksCount);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}
