using System;

namespace Melanchall.DryMidi
{
    public sealed class CopyrightNoticeEvent : MetaEvent
    {
        #region Constructor

        public CopyrightNoticeEvent()
        {
        }

        public CopyrightNoticeEvent(string text)
            : this()
        {
            Text = text;
        }

        #endregion

        #region Properties

        public string Text { get; set; }

        #endregion

        #region Methods

        public bool Equals(CopyrightNoticeEvent copyrightNoticeEvent)
        {
            if (ReferenceEquals(null, copyrightNoticeEvent))
                return false;

            if (ReferenceEquals(this, copyrightNoticeEvent))
                return true;

            return base.Equals(copyrightNoticeEvent) && Text == copyrightNoticeEvent.Text;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Copyright Notice event.");

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
            return new CopyrightNoticeEvent(Text);
        }

        public override string ToString()
        {
            return $"Copyright Notice (text = {Text})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CopyrightNoticeEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (Text?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
