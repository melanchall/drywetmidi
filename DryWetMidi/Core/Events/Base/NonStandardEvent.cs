namespace Melanchall.DryWetMidi.Core
{
    public abstract class NonStandardEvent : MidiEvent
    {
        #region Constructor

        protected NonStandardEvent(MidiEventType eventType)
            : base(eventType)
        {
        }

        #endregion
    }
}
