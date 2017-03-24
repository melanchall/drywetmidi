namespace Melanchall.DryMidi
{
    public sealed class EndOfTrackMessage : MetaMessage
    {
        #region Overrides

        internal override void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1)
        {
        }

        internal override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
        }

        internal override int GetContentSize()
        {
            return 0;
        }

        protected override Message CloneMessage()
        {
            return new EndOfTrackMessage();
        }

        public override string ToString()
        {
            return "End Of Track";
        }

        #endregion
    }
}
