using System;

namespace Melanchall.DryWetMidi
{
    public sealed class CuePointEvent : MetaEvent
    {
        #region Constructor

        public CuePointEvent()
        {
        }

        public CuePointEvent(string text)
            : this()
        {
            Text = text;
        }

        #endregion

        #region Properties

        public string Text { get; set; }

        #endregion

        #region Methods

        public bool Equals(CuePointEvent cuePointEvent)
        {
            if (ReferenceEquals(null, cuePointEvent))
                return false;

            if (ReferenceEquals(this, cuePointEvent))
                return true;

            return base.Equals(cuePointEvent) && Text == cuePointEvent.Text;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Cue Point event.");

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
            return base.GetHashCode() ^ (Text?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
