namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents an Instrument Name meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI instrument name meta message shows the name of the instrument used in the
    /// current track. This optional event is used to provide a textual clue regarding the
    /// intended instrumentation for a track (e.g. 'Piano' or 'Flute', etc). If used, it is
    /// recommended to place this event near the start of a track.
    /// </remarks>
    public sealed class InstrumentNameEvent : BaseTextEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentNameEvent"/>.
        /// </summary>
        public InstrumentNameEvent()
            : base(MidiEventType.InstrumentName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstrumentNameEvent"/> with the
        /// specified instrument name.
        /// </summary>
        /// <param name="instrumentName">Name of the instrument.</param>
        public InstrumentNameEvent(string instrumentName)
            : base(MidiEventType.InstrumentName, instrumentName)
        {
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

        #endregion
    }
}
