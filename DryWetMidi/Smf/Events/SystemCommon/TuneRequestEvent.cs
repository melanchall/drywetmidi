namespace Melanchall.DryWetMidi.Smf
{
    public sealed class TuneRequestEvent : SystemCommonEvent
    {
        #region Overrides

        internal override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
        }

        internal override void Write(MidiWriter writer, WritingSettings settings)
        {
        }

        internal override int GetSize(WritingSettings settings)
        {
            return 0;
        }

        protected override MidiEvent CloneEvent()
        {
            return new TuneRequestEvent();
        }

        public override string ToString()
        {
            return "Tune Request";
        }

        #endregion
    }
}
