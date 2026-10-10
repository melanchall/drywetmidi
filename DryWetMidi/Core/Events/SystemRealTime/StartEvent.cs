namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents Start event.
    /// </summary>
    /// <remarks>
    /// A MIDI event that carries the MIDI start message tells a MIDI slave device to start playback.
    /// </remarks>
    public sealed class StartEvent : SystemRealTimeEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="StartEvent"/>.
        /// </summary>
        public StartEvent()
            : base(MidiEventType.Start)
        {
        }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        protected override MidiEvent CloneEvent()
        {
            return new StartEvent();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Start";
        }

        #endregion
    }
}
