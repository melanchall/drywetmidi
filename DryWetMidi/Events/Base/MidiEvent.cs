using System;

namespace Melanchall.DryWetMidi
{
    /// <summary>
    /// Represents a MIDI file event stored in a track chunk.
    /// </summary>
    public abstract class MidiEvent : ICloneable
    {
        #region Constants

        /// <summary>
        /// Constant for content's size of events that don't have size information stored.
        /// </summary>
        public const int UnknownContentSize = -1;

        #endregion

        #region Fields

        private long _deltaTime;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets delta-time of the event.
        /// </summary>
        /// <remarks>
        /// Delta-time represents the amount of time before the following event. If the first event in a track
        /// occurs at the very beginning of a track, or if two events occur simultaneously, a delta-time of zero is used.
        /// Delta-time is in some fraction of a beat (or a second, for recording a track with SMPTE times), as specified
        /// in the header chunk of the MIDI file.
        /// </remarks>
        public long DeltaTime
        {
            get { return _deltaTime; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value),
                                                          value,
                                                          "Delta-time have to be non-negative number.");

                _deltaTime = value;
            }
        }

        #endregion

        #region Methods

        internal abstract void ReadContent(MidiReader reader, ReadingSettings settings, int size);

        internal abstract void WriteContent(MidiWriter writer, WritingSettings settings);

        internal abstract int GetContentSize();

        protected abstract MidiEvent CloneEvent();

        public bool Equals(MidiEvent midiEvent)
        {
            return Equals(midiEvent, true);
        }

        public bool Equals(MidiEvent midiEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, midiEvent))
                return false;

            if (ReferenceEquals(this, midiEvent))
                return true;

            return !respectDeltaTime || DeltaTime == midiEvent.DeltaTime;
        }

        #endregion

        #region ICloneable

        public object Clone()
        {
            var midiEvent = CloneEvent();
            midiEvent.DeltaTime = DeltaTime;
            return midiEvent;
        }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            return Equals(obj as MidiEvent);
        }

        public override int GetHashCode()
        {
            return DeltaTime.GetHashCode();
        }

        #endregion
    }
}
