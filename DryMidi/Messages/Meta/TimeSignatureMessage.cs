namespace Melanchall.DryMidi
{
    public sealed class TimeSignatureMessage : MetaMessage
    {
        #region Constants

        public const byte DefaultNumerator = 4;
        public const byte DefaultDenominator = 2;
        public const byte DefaultClocks = 24;
        public const byte Default32ndNotesPerBeat = 8;

        #endregion

        #region Constructor

        public TimeSignatureMessage()
        {
        }

        public TimeSignatureMessage(byte numerator, byte denominator, byte clocks, byte numberOf32ndNotesPerBeat)
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

        public bool Equals(TimeSignatureMessage timeSignatureMessage)
        {
            if (ReferenceEquals(null, timeSignatureMessage))
                return false;

            if (ReferenceEquals(this, timeSignatureMessage))
                return true;

            return base.Equals(timeSignatureMessage) && Numerator == timeSignatureMessage.Numerator &&
                                                        Denominator == timeSignatureMessage.Denominator &&
                                                        Clocks == timeSignatureMessage.Clocks &&
                                                        NumberOf32ndNotesPerBeat == timeSignatureMessage.NumberOf32ndNotesPerBeat;
        }

        #endregion

        #region Overrides

        internal override void ReadContent(MidiReader reader, ReadingSettings settings, int size = -1)
        {
            Numerator = reader.ReadByte();
            Denominator = reader.ReadByte();
            Clocks = reader.ReadByte();
            NumberOf32ndNotesPerBeat = reader.ReadByte();
        }

        internal override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(Numerator);
            writer.WriteByte(Denominator);
            writer.WriteByte(Clocks);
            writer.WriteByte(NumberOf32ndNotesPerBeat);
        }

        internal override int GetContentSize()
        {
            return 4;
        }

        protected override Message CloneMessage()
        {
            return new TimeSignatureMessage(Numerator, Denominator, Clocks, NumberOf32ndNotesPerBeat);
        }

        public override string ToString()
        {
            return $"Time Signature (numerator = {Numerator}, denominator = {Denominator}, clocks = {Clocks}, 32nd notes per beat = {NumberOf32ndNotesPerBeat})";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TimeSignatureMessage);
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
