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
