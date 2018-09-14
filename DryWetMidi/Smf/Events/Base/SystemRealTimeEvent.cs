namespace Melanchall.DryWetMidi.Smf
{
    public abstract class SystemRealTimeEvent : MidiEvent
    {
        #region Overrides

        internal override sealed void Read(MidiReader reader, ReadingSettings settings, int size)
        {
        }

        internal override sealed void Write(MidiWriter writer, WritingSettings settings)
        {
        }

        internal override sealed int GetSize(WritingSettings settings)
        {
            return 0;
        }

        #endregion
    }
}
