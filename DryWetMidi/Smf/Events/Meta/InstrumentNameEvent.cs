namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents an Instrument Name meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI instrument name meta message shows the name of the instrument used in the
    /// current track. This optional event is used to provide a textual clue regarding the
    /// intended instrumentation for a track (e.g. 'Piano' or 'Flute', etc). If used, it is
    /// reccommended to place this event near the start of a track.
    /// </remarks>
    public sealed class InstrumentNameEvent : BaseTextEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentNameEvent"/>.
        /// </summary>
        public InstrumentNameEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentNameEvent"/> with the
        /// specified instrument name.
        /// </summary>
        /// <param name="instrumentName">Name of the instrument.</param>
        public InstrumentNameEvent(string instrumentName)
            : base(instrumentName)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="instrumentNameEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(InstrumentNameEvent instrumentNameEvent)
        {
            return Equals(instrumentNameEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="instrumentNameEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(InstrumentNameEvent instrumentNameEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, instrumentNameEvent))
                return false;

            if (ReferenceEquals(this, instrumentNameEvent))
                return true;

            return base.Equals(instrumentNameEvent, respectDeltaTime);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new InstrumentNameEvent(Text);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Instrument Name ({Text})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as InstrumentNameEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
