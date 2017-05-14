namespace Melanchall.DryWetMidi.Smf
{
    public sealed class ProgramChangeEvent : ChannelEvent
    {
        #region Constants

        private const int ParametersCount = 1;
        private const int ProgramNumberParameterIndex = 0;

        #endregion

        #region Constructor

        public ProgramChangeEvent()
            : base(ParametersCount)
        {
        }

        public ProgramChangeEvent(SevenBitNumber programNumber)
            : this()
        {
            ProgramNumber = programNumber;
        }

        #endregion

        #region Properties

        public SevenBitNumber ProgramNumber
        {
            get { return _parameters[ProgramNumberParameterIndex]; }
            set { _parameters[ProgramNumberParameterIndex] = value; }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"Program Change (channel = {Channel}, program number = {ProgramNumber})";
        }

        #endregion
    }
}
