namespace Melanchall.DryWetMidi
{
    public sealed class PitchBendEvent : ChannelEvent
    {
        #region Constants

        private const int ParametersCount = 2;
        private const int PitchValueLsbParameterIndex = 0;
        private const int PitchValueMsbParameterIndex = 1;

        #endregion

        #region Constructor

        public PitchBendEvent()
            : base(ParametersCount)
        {
        }

        public PitchBendEvent(SevenBitNumber pitchValueLsb, SevenBitNumber pitchValueMsb)
            : this()
        {
            PitchValueLsb = pitchValueLsb;
            PitchValueMsb = pitchValueMsb;
        }

        public PitchBendEvent(ushort pitchValue)
            : this()
        {
            PitchValue = pitchValue;
        }

        #endregion

        #region Properties

        public SevenBitNumber PitchValueLsb
        {
            get { return _parameters[PitchValueLsbParameterIndex]; }
            set { _parameters[PitchValueLsbParameterIndex] = value; }
        }

        public SevenBitNumber PitchValueMsb
        {
            get { return _parameters[PitchValueMsbParameterIndex]; }
            set { _parameters[PitchValueMsbParameterIndex] = value; }
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
