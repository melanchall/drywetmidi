namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents Tune Request event.
    /// </summary>
    /// <remarks>
    /// A MIDI event that carries the MIDI tune request message tells a MIDI device to tune itself.
    /// </remarks>
    public sealed class TuneRequestEvent : SystemCommonEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TuneRequestEvent"/>.
        /// </summary>
        public TuneRequestEvent()
            : base(MidiEventType.TuneRequest)
        {
        }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
        }

        internal override void Write(MidiWriter writer, WritingSettings settings)
        {
        }

        internal override int GetSize(WritingSettings settings)
        {
            return 0;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new TuneRequestEvent();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return "Tune Request";
        }

        #endregion
    }
}
