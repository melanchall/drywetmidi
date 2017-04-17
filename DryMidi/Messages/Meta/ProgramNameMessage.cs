using System;

namespace Melanchall.DryMidi
{
    public sealed class ProgramNameMessage : MetaMessage
    {
        #region Constructor

        public ProgramNameMessage()
        {
        }

        public ProgramNameMessage(string text)
            : this()
        {
            Text = text;
        }

        #endregion

        #region Properties

        public string Text { get; set; }

        #endregion

        #region Methods

        public bool Equals(ProgramNameMessage programNameMessage)
        {
            if (ReferenceEquals(null, programNameMessage))
                return false;

            if (ReferenceEquals(this, programNameMessage))
                return true;

            return base.Equals(programNameMessage) && Text == programNameMessage.Text;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Program Name message.");

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
            return new ProgramNameMessage(Text);
        }

        public override string ToString()
        {
            return $"Program Name (text = {Text})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ProgramNameMessage);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (Text?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
