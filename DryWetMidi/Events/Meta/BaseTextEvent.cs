using System;

namespace Melanchall.DryWetMidi
{
    public abstract class BaseTextEvent : MetaEvent
    {
        #region Constructor

        public BaseTextEvent()
        {
        }

        public BaseTextEvent(string text)
            : this()
        {
            Text = text;
        }

        #endregion

        #region Properties

        public string Text { get; set; }

        #endregion

        #region Methods

        public bool Equals(BaseTextEvent baseTextEvent)
        {
            if (ReferenceEquals(null, baseTextEvent))
                return false;

            if (ReferenceEquals(this, baseTextEvent))
                return true;

            return base.Equals(baseTextEvent) && Text == baseTextEvent.Text;
        }

        #endregion

        #region Overrides

        protected override sealed void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read text event.");

            Text = reader.ReadString(size);
        }

        protected override sealed void WriteContentData(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteString(Text);
        }

        protected override sealed int GetContentDataSize()
        {
            return Text?.Length ?? 0;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (Text?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
