namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents Timing Clock event.
    /// </summary>
    /// <remarks>
    /// A MIDI event that carries the MIDI clock message keeps a slave MIDI device
    /// synchronized with a master MIDI device. The MIDI clock message is a timing
    /// message that the master device sends at regular intervals to tell the slave
    /// device where it is in terms of time.
    /// </remarks>
    public sealed class TimingClockEvent : SystemRealTimeEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TimingClockEvent"/>.
        /// </summary>
        public TimingClockEvent()
            : base(MidiEventType.TimingClock)
        {
        }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        protected override MidiEvent CloneEvent()
        {
            return new TimingClockEvent();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Timing Clock";
        }

        #endregion
    }
}
