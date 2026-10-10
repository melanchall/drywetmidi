namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents Stop event.
    /// </summary>
    /// <remarks>
    /// A MIDI event that carries the MIDI stop message tells a MIDI slave device to stop playback.
    /// </remarks>
    public sealed class StopEvent : SystemRealTimeEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="StopEvent"/>.
        /// </summary>
        public StopEvent()
            : base(MidiEventType.Stop)
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
            return new StopEvent();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Stop";
        }

        #endregion
    }
}
