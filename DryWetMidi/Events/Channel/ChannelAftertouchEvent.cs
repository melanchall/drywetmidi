namespace Melanchall.DryWetMidi
{
    public sealed class ChannelAftertouchEvent : ChannelEvent
    {
        #region Constants

        private const int ParametersCount = 1;
        private const int AftertouchValueParameterIndex = 0;

        #endregion

        #region Constructor

        public ChannelAftertouchEvent()
            : base(ParametersCount)
        {
        }

        public ChannelAftertouchEvent(SevenBitNumber aftertouchValue)
            : this()
        {
            AftertouchValue = aftertouchValue;
        }

        #endregion

        #region Properties

        public SevenBitNumber AftertouchValue
        {
            get { return _parameters[AftertouchValueParameterIndex]; }
            set { _parameters[AftertouchValueParameterIndex] = value; }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"Channel Aftertouch (channel = {Channel}, aftertouch value = {AftertouchValue})";
        }

        #endregion
    }
}
