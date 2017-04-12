using System;

namespace Melanchall.DryMidi
{
    public sealed class SmpteOffsetMessage : MetaMessage
    {
        #region Constructor

        public SmpteOffsetMessage()
        {
        }

        public SmpteOffsetMessage(byte hours, byte minutes, byte seconds, byte frames, byte subFrames)
            : this()
        {
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            Frames = frames;
            SubFrames = subFrames;
        }

        #endregion

        #region Properties

        public byte Hours { get; set; }

        public byte Minutes { get; set; }

        public byte Seconds { get; set; }

        public byte Frames { get; set; }

        public byte SubFrames { get; set; }

        #endregion

        #region Methods

        public bool Equals(SmpteOffsetMessage smpteOffsetMessage)
        {
            if (ReferenceEquals(null, smpteOffsetMessage))
                return false;

            if (ReferenceEquals(this, smpteOffsetMessage))
                return true;

            return base.Equals(smpteOffsetMessage) && Hours == smpteOffsetMessage.Hours &&
                                                      Minutes == smpteOffsetMessage.Minutes &&
                                                      Seconds == smpteOffsetMessage.Seconds &&
                                                      Frames == smpteOffsetMessage.Frames &&
                                                      SubFrames == smpteOffsetMessage.SubFrames;
        }

        #endregion

        #region Overrides

        internal override void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1)
        {
            Hours = reader.ReadByte();
            Minutes = reader.ReadByte();
            Seconds = reader.ReadByte();
            Frames = reader.ReadByte();
            SubFrames = reader.ReadByte();
        }

        internal override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Hours);
            writer.WriteByte(Minutes);
            writer.WriteByte(Seconds);
            writer.WriteByte(Frames);
            writer.WriteByte(SubFrames);
        }

        internal override int GetContentSize()
        {
            return 5;
        }

        protected override Message CloneMessage()
        {
            return new SmpteOffsetMessage(Hours, Minutes, Seconds, Frames, SubFrames);
        }

        public override string ToString()
        {
            return $"SMPTE Offset (hours = {Hours}, minutes = {Minutes}, seconds = {Seconds}, frames = {Frames}, subframes = {SubFrames})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SmpteOffsetMessage);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Hours.GetHashCode() ^
                                        Minutes.GetHashCode() ^
                                        Seconds.GetHashCode() ^
                                        Frames.GetHashCode() ^
                                        SubFrames.GetHashCode();
        }

        #endregion
    }
}
