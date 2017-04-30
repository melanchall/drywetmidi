namespace Melanchall.DryWetMidi
{
    public sealed class ProgramChangeEvent : ChannelEvent
    {
        #region Constructor

        public ProgramChangeEvent()
            : base(1)
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
            get { return _parameters[0]; }
            set { _parameters[0] = value; }
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
