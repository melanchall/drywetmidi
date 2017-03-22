namespace Melanchall.DryMidi
{
    public sealed class PitchBendMessage : ChannelMessage
    {
        #region Constructor

        public PitchBendMessage()
            : base(2)
        {
        }

        public PitchBendMessage(SevenBitNumber pitchValueLsb, SevenBitNumber pitchValueMsb)
            : this()
        {
            PitchValueLsb = pitchValueLsb;
            PitchValueMsb = pitchValueMsb;
        }

        public PitchBendMessage(ushort pitchValue)
            : this()
        {
            PitchValue = pitchValue;
        }

        #endregion

        #region Properties

        public SevenBitNumber PitchValueLsb
        {
            get { return _parameters[0]; }
            set { _parameters[0] = value; }
        }

        public SevenBitNumber PitchValueMsb
        {
            get { return _parameters[1]; }
            set { _parameters[1] = value; }
        }

        public ushort PitchValue
        {
            get { return DataTypesUtilities.Combine(PitchValueLsb, PitchValueMsb); }
            set
            {
                PitchValueLsb = value.GetHead();
                PitchValueMsb = value.GetTail();
            }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"Pitch Bend (channel = {Channel}, pitch value LSB = {PitchValueLsb}, pitch value MSB = {PitchValueMsb})";
        }

        #endregion
    }
}
