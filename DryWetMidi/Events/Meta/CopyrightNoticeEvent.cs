namespace Melanchall.DryWetMidi
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

        public bool Equals(CopyrightNoticeEvent copyrightNoticeEvent)
        {
            return Equals(copyrightNoticeEvent, true);
        }

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

        protected override MidiEvent CloneEvent()
        {
            return new CopyrightNoticeEvent(Text);
        }

        public override string ToString()
        {
            return $"Copyright Notice (text = {Text})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CopyrightNoticeEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
