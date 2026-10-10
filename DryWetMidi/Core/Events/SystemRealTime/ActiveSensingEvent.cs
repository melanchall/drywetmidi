namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents Active Sensing event.
    /// </summary>
    /// <remarks>
    /// A MIDI event that carries the MIDI active sense message tells a MIDI device
    /// that the MIDI connection is still active.
    /// </remarks>
    public sealed class ActiveSensingEvent : SystemRealTimeEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ActiveSensingEvent"/>.
        /// </summary>
        public ActiveSensingEvent()
            : base(MidiEventType.ActiveSensing)
        {
        }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        protected override MidiEvent CloneEvent()
        {
            return new ActiveSensingEvent();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Active Sensing";
        }

        #endregion
    }
}
