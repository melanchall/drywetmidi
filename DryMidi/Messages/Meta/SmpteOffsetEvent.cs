using System;

namespace Melanchall.DryMidi
{
    public sealed class SmpteOffsetEvent : MetaEvent
    {
        #region Constructor

        public SmpteOffsetEvent()
        {
        }

        public SmpteOffsetEvent(byte hours, byte minutes, byte seconds, byte frames, byte subFrames)
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

        public bool Equals(SmpteOffsetEvent smpteOffsetEvent)
        {
            if (ReferenceEquals(null, smpteOffsetEvent))
                return false;

            if (ReferenceEquals(this, smpteOffsetEvent))
                return true;

            return base.Equals(smpteOffsetEvent) && Hours == smpteOffsetEvent.Hours &&
                                                    Minutes == smpteOffsetEvent.Minutes &&
                                                    Seconds == smpteOffsetEvent.Seconds &&
                                                    Frames == smpteOffsetEvent.Frames &&
                                                    SubFrames == smpteOffsetEvent.SubFrames;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            Hours = reader.ReadByte();
            Minutes = reader.ReadByte();
            Seconds = reader.ReadByte();
            Frames = reader.ReadByte();
            SubFrames = reader.ReadByte();
        }

        protected override void WriteContentData(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Hours);
            writer.WriteByte(Minutes);
            writer.WriteByte(Seconds);
            writer.WriteByte(Frames);
            writer.WriteByte(SubFrames);
        }

        protected override int GetContentDataSize()
        {
            return 5;
        }

        protected override MidiEvent CloneEvent()
        {
            return new SmpteOffsetEvent(Hours, Minutes, Seconds, Frames, SubFrames);
        }

        public override string ToString()
        {
            return $"SMPTE Offset (hours = {Hours}, minutes = {Minutes}, seconds = {Seconds}, frames = {Frames}, subframes = {SubFrames})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SmpteOffsetEvent);
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
