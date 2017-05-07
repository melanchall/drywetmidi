namespace Melanchall.DryWetMidi
{
    public sealed class MarkerEvent : BaseTextEvent
    {
        #region Constructor

        public MarkerEvent()
        {
        }

        public MarkerEvent(string text)
            : base(text)
        {
        }

        #endregion

        #region Methods

        public bool Equals(MarkerEvent markerEvent)
        {
            return Equals(markerEvent, true);
        }

        public bool Equals(MarkerEvent markerEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, markerEvent))
                return false;

            if (ReferenceEquals(this, markerEvent))
                return true;

            return base.Equals(markerEvent, respectDeltaTime);
        }

        #endregion

        #region Overrides

        protected override MidiEvent CloneEvent()
        {
            return new MarkerEvent(Text);
        }

        public override string ToString()
        {
            return $"Marker (text = {Text})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MarkerEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
