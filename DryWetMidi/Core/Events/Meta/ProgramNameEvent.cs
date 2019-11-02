namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a Program Name meta event.
    /// </summary>
    /// <remarks>
    /// This optional event is used to embed the patch/program name that is called up by the
    /// immediately subsequent Bank Select and Program Change messages. It serves to aid the
    /// end user in making an intelligent program choice when using different hardware.
    /// </remarks>
    public sealed class ProgramNameEvent : BaseTextEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramNameEvent"/>.
        /// </summary>
        public ProgramNameEvent()
            : base(MidiEventType.ProgramName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramNameEvent"/> with the
        /// specified program name.
        /// </summary>
        /// <param name="programName">Name of the program.</param>
        public ProgramNameEvent(string programName)
            : base(MidiEventType.ProgramName, programName)
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
            return new ProgramNameEvent(Text);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Program Name ({Text})";
        }

        #endregion
    }
}
