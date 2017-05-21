namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Reprsents an "escape" system exclusive event which defines an escape sequence.
    /// </summary>
    /// <remarks>
    /// "Escape" system exclisive events start with 0xF7 byte and don't have a terminal 0xF7
    /// byte that is required for normal sysex events.
    /// When an "escape" sysex event is encountered whilst reading a MIDI file, its interpretation
    /// (SysEx packet or escape sequence) is determined as follows:
    /// - When an event with 0xF0 status but lacking a terminal 0xF7 is encountered, then this is the
    ///   first of a Casio-style multi-packet message, and a flag (boolean variable) should be set to
    ///   indicate this.
    /// - If an event with 0xF7 status is encountered whilst this flag is set, then this is a continuation
    ///   event (a system exclusive packet, one of many). If this event has a terminal 0xF7, then it is
    ///   the last packet and flag should be cleared.
    /// - If an event with 0xF7 status is encountered whilst flag is clear, then this event is an escape sequence.
    /// </remarks>
    public sealed class EscapeSysExEvent : SysExEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="EscapeSysExEvent"/>.
        /// </summary>
        public EscapeSysExEvent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EscapeSysExEvent"/> with the
        /// specified data.
        /// </summary>
        /// <param name="data">Data of the "escape" sysex event.</param>
        public EscapeSysExEvent(byte[] data)
        {
            Data = data;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="escapeSysExEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(EscapeSysExEvent escapeSysExEvent)
        {
            return Equals(escapeSysExEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="escapeSysExEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(EscapeSysExEvent escapeSysExEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, escapeSysExEvent))
                return false;

            if (ReferenceEquals(this, escapeSysExEvent))
                return true;

            return base.Equals(escapeSysExEvent, respectDeltaTime);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return "Escape SysEx";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as EscapeSysExEvent);
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
