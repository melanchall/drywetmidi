using System;

namespace Melanchall.DryMidi
{
    public sealed class MarkerEvent : MetaEvent
    {
        #region Constructor

        public MarkerEvent()
        {
        }

        public MarkerEvent(string text)
            : this()
        {
            Text = text;
        }

        #endregion

        #region Properties

        public string Text { get; set; }

        #endregion

        #region Methods

        public bool Equals(MarkerEvent markerEvent)
        {
            if (ReferenceEquals(null, markerEvent))
                return false;

            if (ReferenceEquals(this, markerEvent))
                return true;

            return base.Equals(markerEvent) && Text == markerEvent.Text;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            if (size < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(size),
                    size,
                    "Non-negative size have to be specified in order to read Marker event.");

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
            return new MarkerEvent(Text);
        }

        public override string ToString()
        {
            return $"Marker (text = {Text})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MarkerEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ (Text?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
