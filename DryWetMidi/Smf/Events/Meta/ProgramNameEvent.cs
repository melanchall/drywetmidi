namespace Melanchall.DryWetMidi.Smf
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
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramNameEvent"/> with the
        /// specified program name.
        /// </summary>
        /// <param name="programName">Name of the program.</param>
        public ProgramNameEvent(string programName)
            : base(programName)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="programNameEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(ProgramNameEvent programNameEvent)
        {
            return Equals(programNameEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="programNameEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(ProgramNameEvent programNameEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, programNameEvent))
                return false;

            if (ReferenceEquals(this, programNameEvent))
                return true;

            return base.Equals(programNameEvent, respectDeltaTime);
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

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ProgramNameEvent);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
