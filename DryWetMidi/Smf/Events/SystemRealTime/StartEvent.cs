namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents Start event.
    /// </summary>
    /// <remarks>
    /// A MIDI event that carries the MIDI start message tells a MIDI slave device to start playback.
    /// </remarks>
    public sealed class StartEvent : SystemRealTimeEvent
    {
        #region Overrides

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new StartEvent();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return "Start";
        }

        #endregion
    }
}
