namespace Melanchall.DryWetMidi.Core
{
    public abstract class CustomMetaEvent : MetaEvent
    {
        #region Constructor

        public CustomMetaEvent()
            : base(MidiEventType.CustomMeta)
        {
        }

        #endregion
    }
}
