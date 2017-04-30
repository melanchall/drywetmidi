namespace Melanchall.DryWetMidi
{
    public sealed class TimeSignatureEvent : MetaEvent
    {
        #region Constants

        public const byte DefaultNumerator = 4;
        public const byte DefaultDenominator = 2;
        public const byte DefaultClocks = 24;
        public const byte Default32ndNotesPerBeat = 8;

        #endregion

        #region Constructor

        public TimeSignatureEvent()
        {
        }

        public TimeSignatureEvent(byte numerator, byte denominator, byte clocks, byte numberOf32ndNotesPerBeat)
            : this()
        {
            Numerator = numerator;
            Denominator = denominator;
            Clocks = clocks;
            NumberOf32ndNotesPerBeat = numberOf32ndNotesPerBeat;
        }

        #endregion

        #region Properties

        public byte Numerator { get; set; } = DefaultNumerator;

        public byte Denominator { get; set; } = DefaultDenominator;

        public byte Clocks { get; set; } = DefaultClocks;

        public byte NumberOf32ndNotesPerBeat { get; set; } = Default32ndNotesPerBeat;

        #endregion

        #region Methods

        public bool Equals(TimeSignatureEvent timeSignatureEvent)
        {
            if (ReferenceEquals(null, timeSignatureEvent))
                return false;

            if (ReferenceEquals(this, timeSignatureEvent))
                return true;

            return base.Equals(timeSignatureEvent) && Numerator == timeSignatureEvent.Numerator &&
                                                      Denominator == timeSignatureEvent.Denominator &&
                                                      Clocks == timeSignatureEvent.Clocks &&
                                                      NumberOf32ndNotesPerBeat == timeSignatureEvent.NumberOf32ndNotesPerBeat;
        }

        #endregion

        #region Overrides

        protected override void ReadContentData(MidiReader reader, ReadingSettings settings, int size)
        {
            Numerator = reader.ReadByte();
            Denominator = reader.ReadByte();
            Clocks = reader.ReadByte();
            NumberOf32ndNotesPerBeat = reader.ReadByte();
        }

        protected override void WriteContentData(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Numerator);
            writer.WriteByte(Denominator);
            writer.WriteByte(Clocks);
            writer.WriteByte(NumberOf32ndNotesPerBeat);
        }

        protected override int GetContentDataSize()
        {
            return 4;
        }

        protected override MidiEvent CloneEvent()
        {
            return new TimeSignatureEvent(Numerator, Denominator, Clocks, NumberOf32ndNotesPerBeat);
        }

        public override string ToString()
        {
            return $"Time Signature (numerator = {Numerator}, denominator = {Denominator}, clocks = {Clocks}, 32nd notes per beat = {NumberOf32ndNotesPerBeat})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TimeSignatureEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Numerator.GetHashCode() ^
                                        Denominator.GetHashCode() ^
                                        Clocks.GetHashCode() ^
                                        NumberOf32ndNotesPerBeat.GetHashCode();
        }

        #endregion
    }
}
