using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Smf
{
    [Serializable]
    public sealed class TooManyTrackChunksException : MidiException
    {
        #region Constants

        private const string TrackChunksCountSerializationPropertyName = "TrackChunksCount";

        #endregion

        #region Constructors

        public TooManyTrackChunksException()
            : base()
        {
        }

        public TooManyTrackChunksException(string message, int trackChunksCount)
            : base(message)
        {
            TrackChunksCount = trackChunksCount;
        }

        private TooManyTrackChunksException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            TrackChunksCount = info.GetInt32(TrackChunksCountSerializationPropertyName);
        }

        #endregion

        #region Properties

        public int TrackChunksCount { get; }

        #endregion

        #region Overrides

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue(TrackChunksCountSerializationPropertyName, TrackChunksCount);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}
