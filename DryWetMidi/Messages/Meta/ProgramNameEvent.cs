using System;

namespace Melanchall.DryWetMidi
{
    public sealed class ProgramNameEvent : MetaEvent
    {
        #region Constructor

        public ProgramNameEvent()
        {
        }

        public ProgramNameEvent(string text)
            : this()
        {
            Text = text;
        }

        #endregion

        #region Properties

        public string Text { get; set; }

        #endregion

        #region Methods

        public bool Equals(ProgramNameEvent programNameEvent)
        {
            if (ReferenceEquals(null, programNameEvent))
                return false;

            if (ReferenceEquals(this, programNameEvent))
                return true;

            return base.Equals(programNameEvent) && Text == programNameEvent.Text;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Program Name event.");

            Text = reader.ReadString(size);
        }

        protected override void WriteContentData(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteString(Text);
        }

        protected override int GetContentDataSize()
        {
            return Text?.Length ?? 0;
        }

        protected override MidiEvent CloneEvent()
        {
            return new ProgramNameEvent(Text);
        }

        public override string ToString()
        {
            return $"Program Name (text = {Text})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ProgramNameEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (Text?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
