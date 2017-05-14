namespace Melanchall.DryWetMidi.Smf
{
    public sealed class ControlChangeEvent : ChannelEvent
    {
        #region Constants

        private const int ParametersCount = 2;
        private const int ControlNumberParameterIndex = 0;
        private const int ControlValueParameterIndex = 1;

        #endregion

        #region Constructor

        public ControlChangeEvent()
            : base(ParametersCount)
        {
        }

        public ControlChangeEvent(SevenBitNumber controlNumber, SevenBitNumber controlValue)
            : this()
        {
            ControlNumber = controlNumber;
            ControlValue = controlValue;
        }

        #endregion

        #region Properties

        public SevenBitNumber ControlNumber
        {
            get { return _parameters[ControlNumberParameterIndex]; }
            set { _parameters[ControlNumberParameterIndex] = value; }
        }

        public SevenBitNumber ControlValue
        {
            get { return _parameters[ControlValueParameterIndex]; }
            set { _parameters[ControlValueParameterIndex] = value; }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"Control Change (channel = {Channel}, control number = {ControlNumber}, control value = {ControlValue})";
        }

        #endregion
    }
}
