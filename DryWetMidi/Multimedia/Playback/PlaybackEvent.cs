using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class PlaybackEvent : IEquatable<PlaybackEvent>
    {
        #region Constructor

        public PlaybackEvent(
            MidiEvent midiEvent,
            TimeSpan time,
            long rawTime,
            ITimedObject objectReference)
        {
            Event = midiEvent;
            Time = time;
            RawTime = rawTime;
            ObjectReference = objectReference;
        }

        #endregion

        #region Properties

        public MidiEvent Event { get; }

        public TimeSpan Time { get; }

        public long RawTime { get; }

        public ITimedObject ObjectReference { get; }

        public ICollection<RedBlackTreeNode<TimeSpan, PlaybackEvent>> EventsGroup { get; set; }

        public PlaybackEventMetadata Metadata { get; } = new PlaybackEventMetadata();

        #endregion

        #region IEquatable<PlaybackEvent>

        public bool Equals(PlaybackEvent other)
        {
            return object.ReferenceEquals(this, other);
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"{Time}: {Event}";
        }

        #endregion
    }
}
