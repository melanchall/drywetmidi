namespace Melanchall.DryWetMidi.Smf
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

        public PitchBendEvent(ushort pitchValue)
            : this()
        {
            PitchValue = pitchValue;
        }

        #endregion

        #region Properties

        public ushort PitchValue
        {
            get { return DataTypesUtilities.Combine(PitchValueLsb, PitchValueMsb); }
            set
            {
                PitchValueLsb = value.GetHead();
                PitchValueMsb = value.GetTail();
            }
        }

        private SevenBitNumber PitchValueLsb
        {
            get { return _parameters[PitchValueLsbParameterIndex]; }
            set { _parameters[PitchValueLsbParameterIndex] = value; }
        }

        private SevenBitNumber PitchValueMsb
        {
            get { return _parameters[PitchValueMsbParameterIndex]; }
            set { _parameters[PitchValueMsbParameterIndex] = value; }
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
