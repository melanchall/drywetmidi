using System;

namespace Melanchall.DryMidi
{
    public sealed class CopyrightNoticeMessage : MetaMessage
    {
        #region Constructor

        public CopyrightNoticeMessage()
        {
        }

        public CopyrightNoticeMessage(string text)
            : this()
        {
            Text = text;
        }

        #endregion

        #region Properties

        public string Text { get; set; }

        #endregion

        #region Methods

        public bool Equals(CopyrightNoticeMessage copyrightNoticeMessage)
        {
            if (ReferenceEquals(null, copyrightNoticeMessage))
                return false;

            if (ReferenceEquals(this, copyrightNoticeMessage))
                return true;

            return base.Equals(copyrightNoticeMessage) && Text == copyrightNoticeMessage.Text;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Copyright Notice message.");

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

        protected override Message CloneMessage()
        {
            return new CopyrightNoticeMessage(Text);
        }

        public override string ToString()
        {
            return $"Copyright Notice (text = {Text})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as CopyrightNoticeMessage);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (Text?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
