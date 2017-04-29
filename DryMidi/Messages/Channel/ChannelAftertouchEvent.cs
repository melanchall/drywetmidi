namespace Melanchall.DryMidi
{
    public sealed class ChannelAftertouchEvent : ChannelEvent
    {
        #region Constructor

        public ChannelAftertouchEvent()
            : base(1)
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
            get { return _parameters[0]; }
            set { _parameters[0] = value; }
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
