namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Represents a Pitch Bend Change message.
    /// </summary>
    /// <remarks>
    /// This message is sent to indicate a change in the pitch bender (wheel or lever, typically).
    /// The pitch bender is measured by a fourteen bit value. Center (no pitch change) is 0x2000.
    /// </remarks>
    public sealed class PitchBendEvent : ChannelEvent
    {
        #region Constants

        private const int ParametersCount = 2;
        private const int PitchValueLsbParameterIndex = 0;
        private const int PitchValueMsbParameterIndex = 1;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="PitchBendEvent"/>.
        /// </summary>
        public PitchBendEvent()
            : base(ParametersCount)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PitchBendEvent"/> with the specified
        /// pitch value.
        /// </summary>
        /// <param name="pitchValue">Pitch value.</param>
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
