using System;

namespace Melanchall.DryMidi
{
    public sealed class MarkerMessage : MetaMessage
    {
        #region Constructor

        public MarkerMessage()
        {
        }

        public MarkerMessage(string text)
            : this()
        {
            Text = text;
        }

        #endregion

        #region Properties

        public string Text { get; set; }

        #endregion

        #region Methods

        public bool Equals(MarkerMessage markerMessage)
        {
            if (ReferenceEquals(null, markerMessage))
                return false;

            if (ReferenceEquals(this, markerMessage))
                return true;

            return base.Equals(markerMessage) && Text == markerMessage.Text;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Marker message.");

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
            return new MarkerMessage(Text);
        }

        public override string ToString()
        {
            return $"Marker (text = {Text})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MarkerMessage);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (Text?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
