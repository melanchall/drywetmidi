using System;

namespace Melanchall.DryWetMidi
{
    public sealed class CuePointEvent : BaseTextEvent
    {
        #region Constructor

        public CuePointEvent()
        {
        }

        public CuePointEvent(string text)
            : base(text)
        {
        }

        #endregion

        #region Methods

        public bool Equals(CuePointEvent cuePointEvent)
        {
            return Equals(cuePointEvent, true);
        }

        public bool Equals(CuePointEvent cuePointEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, cuePointEvent))
                return false;

            if (ReferenceEquals(this, cuePointEvent))
                return true;

            return base.Equals(cuePointEvent, respectDeltaTime);
        }

        #endregion

        #region Overrides

        protected override MidiEvent CloneEvent()
        {
            return new CuePointEvent(Text);
        }

        public override string ToString()
        {
            return $"Cue Point (text = {Text})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CuePointEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
