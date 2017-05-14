namespace Melanchall.DryWetMidi.Smf
{
    public sealed class CopyrightNoticeEvent : BaseTextEvent
    {
        #region Constructor

        public CopyrightNoticeEvent()
        {
        }

        public CopyrightNoticeEvent(string text)
            : base(text)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="copyrightNoticeEvent">The event to compare with the current one.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(CopyrightNoticeEvent copyrightNoticeEvent)
        {
            return Equals(copyrightNoticeEvent, true);
        }

        /// <summary>
        /// Determines whether the specified event is equal to the current one.
        /// </summary>
        /// <param name="copyrightNoticeEvent">The event to compare with the current one.</param>
        /// <param name="respectDeltaTime">If true the <see cref="MidiEvent.DeltaTime"/> will be taken into an account
        /// while comparing events; if false - delta-times will be ignored.</param>
        /// <returns>true if the specified event is equal to the current one; otherwise, false.</returns>
        public bool Equals(CopyrightNoticeEvent copyrightNoticeEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, copyrightNoticeEvent))
                return false;

            if (ReferenceEquals(this, copyrightNoticeEvent))
                return true;

            return base.Equals(copyrightNoticeEvent, respectDeltaTime);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new CopyrightNoticeEvent(Text);
        }

        public override string ToString()
        {
            return $"Copyright Notice (text = {Text})";
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as CopyrightNoticeEvent);
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
