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

        /// <inheritdoc/>
        protected override MidiEvent CloneEvent()
        {
            return new ContinueEvent();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Continue";
        }

        #endregion
    }
}
