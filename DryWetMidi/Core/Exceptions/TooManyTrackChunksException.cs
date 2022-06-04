using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown while writing a MIDI file when the <see cref="MidiFile.Chunks"/>
    /// contains more than <see cref="ushort.MaxValue"/> track chunks which is the maximum allowed count for
    /// chunks of this type.
    /// </summary>
    [Serializable]
    public sealed class TooManyTrackChunksException : MidiException
    {
        #region Constructors

        internal TooManyTrackChunksException(int trackChunksCount)
            : base($"Count of track chunks to be written ({trackChunksCount}) is greater than the valid maximum ({ushort.MaxValue}).")
        {
            TrackChunksCount = trackChunksCount;
        }

        private TooManyTrackChunksException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            TrackChunksCount = info.GetInt32(nameof(TrackChunksCount));
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
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <c>null</c>.</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(TrackChunksCount), TrackChunksCount);
        }

        #endregion
    }
}
