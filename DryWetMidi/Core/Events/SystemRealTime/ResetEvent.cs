namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents Reset event.
    /// </summary>
    /// <remarks>
    /// A MIDI event that carries the MIDI reset message tells a MIDI device to reset itself.
    /// </remarks>
    public sealed class ResetEvent : SystemRealTimeEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ResetEvent"/>.
        /// </summary>
        public ResetEvent()
            : base(MidiEventType.Reset)
        {
        }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        protected override MidiEvent CloneEvent()
        {
            return new ResetEvent();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Reset";
        }

        #endregion
    }
}
