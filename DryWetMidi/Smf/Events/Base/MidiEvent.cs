using System;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a MIDI file event stored in a track chunk.
    /// </summary>
    public abstract class MidiEvent
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
        /// by the file's time division.
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

        /// <summary>
        /// Reads content of a MIDI event.
        /// </summary>
        /// <param name="reader">Reader to read the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        internal abstract void Read(MidiReader reader, ReadingSettings settings, int size);

        /// <summary>
        /// Writes content of a MIDI event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        internal abstract void Write(MidiWriter writer, WritingSettings settings);

        /// <summary>
        /// Gets the size of the content of a MIDI event.
        /// </summary>
        /// <returns>Size of the event's content.</returns>
        internal abstract int GetSize();

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected abstract MidiEvent CloneEvent();

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        public MidiEvent Clone()
        {
            var midiEvent = CloneEvent();
            midiEvent.DeltaTime = DeltaTime;
            return midiEvent;
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="midiEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(MidiEvent midiEvent)
        {
            return Equals(midiEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="midiEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(MidiEvent midiEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, midiEvent))
                return false;

            if (ReferenceEquals(this, midiEvent))
                return true;

            return !respectDeltaTime || DeltaTime == midiEvent.DeltaTime;
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as MidiEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return DeltaTime.GetHashCode();
        }

        #endregion
    }
}
