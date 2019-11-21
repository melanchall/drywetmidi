namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents Continue event.
    /// </summary>
    /// <remarks>
    /// A MIDI event that carries the MIDI continue message tells a MIDI slave device to resume playback.
    /// </remarks>
    public sealed class ContinueEvent : SystemRealTimeEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ContinueEvent"/>.
        /// </summary>
        public ContinueEvent()
            : base(MidiEventType.Continue)
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
            return new ContinueEvent();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return "Continue";
        }

        #endregion
    }
}
